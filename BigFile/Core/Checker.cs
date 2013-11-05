namespace BigFile.Core
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public class Checker : IDisposable
    {

        private readonly ReaderWriterLockSlim lockS = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        private readonly String fileName;

        private Int32 inTune;

        private Int32 offset;

        private Int32 innerOffset;

        private Boolean firstCall = true;

        private Int32 count;

        private WeakReference hashRef;

        public String firstSubline { get; private set; }

        private String lastSubline;

        public Checker(string fileName, Int32 offset, Int32 count)
        {
            this.fileName = fileName;
            this.offset = offset;
            this.innerOffset = 0;
            this.count = count;
            this.inTune = 0;
            this.hashRef = new WeakReference(null, false);
        }

        public Tuner Check(String searchLine, Result result, Tuner prevTuner)
        {
           
            Tuner thisTuner = inTune == 0 ? new Tuner(this, searchLine, result) : null;
            //блокировок еще нет и тюнинг может измениться но обрабатываться строка будет тюнингом

            Task.Factory.StartNew(() =>
            {
                CancellationToken token = result.GetToken();
                if (token.IsCancellationRequested)
                {
                    return;
                }
                
                var hashtable = hashRef.Target as Hashtable; //root захвачен таблица не может быть уничтожена
                if (hashtable == null)
                {
                    //это первый проход или проход или проход с разрушенной таблицей (но с известной первой и последней строкой)
                    lockS.EnterWriteLock();
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }
                    hashtable = this.Parse(searchLine, result, thisTuner, prevTuner, token);
                    this.hashRef.Target = hashtable;
                    lockS.ExitWriteLock();
                }
                else
                {
                    //это уже не первые проход и мы прошли коллекцию до конца 
                    //знаем первую и последнюю строку

                    lockS.EnterReadLock();
                    //если чекер успел стать интуном можем уничтожить тюнер!!!!!!!!!!!! и прочитать все из таблицы
                    if (this.inTune == 1)
                    {
                        thisTuner = null;
                    }
                    if (hashtable.Contains(searchLine))
                    {
                        result.Increace((int)hashtable[searchLine]);
                    }
                    lockS.ExitReadLock();

                }

                if (prevTuner != null)
                {
                    prevTuner.SetSecond(this.firstSubline);
                }

                if (thisTuner != null)
                {
                    //если уж тюнер создан и дожил досюда выполняем
                    //hashtable не может быть уничтожена пока содержится в тюнере
                    //если тюнер созавался то только он может проверить последнн значение
                    //и хештаблицу изменит он (или первый один из них)
                    thisTuner.SetFirst(hashtable); // передача захвата таблицы тюнеру
                }
            });

            return thisTuner;

        }

        private Hashtable Parse(String searchLine, Result result, Tuner thisTuner, Tuner prevTuner, CancellationToken token)
        {
            //посмотреть не надо ли отменить
            //Еще одна проверка может таблица уже создана пока стояля блокировка тогда вернуть и чуть тюнеры подправить
            var hashtable = hashRef.Target as Hashtable;
            if (hashtable != null)
            {
                //Ура таблица успела появиться, а значит первое и последнее значение получены
                Console.WriteLine("YOO");
                if (hashtable.Contains(searchLine))
                {
                    result.Increace((int)hashtable[searchLine]);
                }
                return hashtable;

            }
            else
            {
                hashtable = new Hashtable();
                using (Stream stream = this.GetStream())
                {
                    using (StreamReader streamReader = new StreamReader(stream))
                    {
                        string line = streamReader.ReadLine();

                        if (this.firstCall && prevTuner != null)
                        {
                            //первая строка устанавливается при первом проходе
                            this.firstSubline = line;
                            this.innerOffset = String.IsNullOrEmpty(line) ? 0 : line.Length;
                        }
                        else
                        {
                            //это первый вызов превого чекера или внутренний сдвиг уде остановлен - строку обрабатываем сами
                            //первая строка и сдвиг уже устанавлены при первом (даже неудачном проходе)
                            MakeDeepCheck(line, searchLine, result, hashtable);
                        }
                        this.firstCall = false;
                        //до этого момента нельзя отменять выполнение

                        Boolean done = streamReader.EndOfStream;
                        while (!done)
                        {
                            if (token.IsCancellationRequested)
                            {
                                Console.WriteLine("Canceled");
                                thisTuner = null;
                                return null;
                            }

                            line = streamReader.ReadLine();

                            if (!streamReader.EndOfStream)
                            {
                                //обрабатываем все строки кроме последней
                                MakeDeepCheck(line, searchLine, result, hashtable);
                            }
                            else
                            {
                                done = true;
                            }

                        }

                        if (this.inTune == 0)
                        {
                            //запоминаем послежнюю строку
                            this.lastSubline = line;
                        }

                        if (thisTuner == null)
                        {
                            //если тюнера нет то он уже увеличил количество байтов в массиве 
                            //и этот код вызывается при разрушении табцицы
                            MakeDeepCheck(line, searchLine, result, hashtable); //we can have tuners with intune mode
                        }
                    }

                }
            }

            return hashtable;
        }

        public void Tune(String nextCheckerFirstSubline, String searchLine, Result result, Hashtable thisHashtable)
        {
            String concantinatedLine = lastSubline + nextCheckerFirstSubline;

            if (Interlocked.CompareExchange(ref this.inTune, 1, 0) == 0)
            {
                //код выполняется только раз
                lockS.EnterWriteLock();
                this.count = this.count + nextCheckerFirstSubline.Length;
                //var hashtable = thisHashtable; // !!! ref has hashtable реинкорнация таблицы (но ссылка по идее еще есть просто в hashref)
                MakeDeepCheck(concantinatedLine, searchLine, result, thisHashtable);
                //this.hashRef.Target = hashtable;
                lockS.ExitWriteLock();
                //this.hashtable = null;// рано надо еще установить слабую ссылку 

            }
            else
            {
                MakeDeepCheck(concantinatedLine, searchLine, result, null); // check but not update hashtable
            }

        }

        private void MakeDeepCheck(String line, String searchLine, Result result, Hashtable hashtable)
        {

            //Console.WriteLine(".{0}.", line);
            if (result != null && String.Equals(line, searchLine))
            {
                result.Increace();

            }

            if (hashtable != null)
            {
                Object prevValue = hashtable[line];
                hashtable[line] = prevValue == null ? 1 : (Int32)prevValue + 1;
            }
        }

        private Stream GetStream()
        {
            Byte[] buffer = new Byte[count];
            using (FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read, count))
            {
                fileStream.Seek(offset + innerOffset, SeekOrigin.Begin);
                var readed = fileStream.Read(buffer, 0, this.count - innerOffset);
                this.count = readed; // отбрасываем пустые в конце
            }
            return new MemoryStream(buffer, 0, this.count);
        }

        public void Dispose()
        {
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace VGUILocalizationTool
{
    class ValveLocalizationFile
    {
        private string englishFile;
        public ValveLocalizationFile(string englishFile)
        {
            this.englishFile = englishFile;
        }

        public bool WithoutLang { get; set; }
        public bool WithEnglishText { get; set; }
        public bool DontSaveNotLocalized { get; set; }

        private string GetLocalFileName(string local)
        {
            string fileName = Path.GetFileNameWithoutExtension(englishFile);
            int pos = fileName.IndexOf("_english");
            if (pos >= 0)
            {
                fileName = fileName.Remove(pos);
                pos++;
            }
            string ext = Path.GetExtension(englishFile);
            return Path.GetDirectoryName(englishFile) + "\\" + fileName + "_" + local + ext;
        }

        string[] SplitWithQuotas(string str, ref bool unterm)
        {
            LinkedList<string> list = new LinkedList<string>();
            bool q = false;
            bool s = false;
            StringBuilder sb = new StringBuilder();
            char o = '\0';
            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];
                if (!q)
                {
                    if (c == '\"' && o != '\\')
                    {
                        if (sb.Length > 0)
                        {
                            list.AddLast(sb.ToString());
                            sb.Length = 0;
                        }
                        q = true;
                        s = false;
                    }
                    else if (c != ' ' && c != '\t')
                    {
                        if (s && sb.Length > 0)
                        {
                            list.AddLast(sb.ToString());
                            sb.Length = 0;
                        }
                        sb.Append(c);
                        s = false;
                    }
                    else
                    {
                        if (!s && sb.Length > 0)
                        {
                            list.AddLast(sb.ToString());
                            sb.Length = 0;
                        }
                        sb.Append(c);
                        s = true;
                    }
                }
                else
                {
                    if (c == '\"' && o != '\\')
                    {
                        q = false;
                        list.AddLast(sb.ToString());
                        sb.Length = 0;
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
                o = c;
            }
            if (q || sb.Length != 0)
            {
                list.AddLast(sb.ToString());
            }
            unterm = q;
            return list.ToArray();
        }

        private int SkipEmpty(string[] tokens, int start)
        {
            for (int i = start; i < tokens.Length; i++)
            {
                if (tokens[i].Trim().Length > 0)
                {
                    return i;
                }
            }
            return -1;
        }

        public List<LocalizationData> ReadData(string local)
        {
            WithoutLang = false;
            WithEnglishText = false;
            List<LocalizationData> data = new List<LocalizationData>();
            string fileName = GetLocalFileName(local);
            int l = 0;
            //try
            {
                using (StreamReader sr = new StreamReader(fileName))
                {
                    String line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        bool unterm = false;
                        string[] tokens = SplitWithQuotas(line, ref unterm);
                        bool ML = false;
                        if (unterm)
                        {
                            bool q = false;
                            int idx = tokens.Length - 1;
                            do
                            {
                                line = sr.ReadLine();
                                if (line == null)
                                {
                                    break;
                                }
                                ML = true;
                                if (line.Length > 0)
                                {
                                    for (int ci = line.Length - 1; ci >= 0; ci--)
                                    {
                                        if (line[ci] == ' ' || line[ci] == '\t')
                                        {
                                            continue;
                                        }
                                        else if (line[ci] == '\"' && (ci == 0 || line[ci - 1] != '\\'))
                                        {
                                            q = true;
                                            tokens[idx] += Environment.NewLine + line.Substring(0, ci);
                                            break;
                                        }
                                        else
                                        {
                                            q = false;
                                            tokens[idx] += Environment.NewLine + line;
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    tokens[idx] += Environment.NewLine + line;
                                }
                            } while (!q);
                        }
                        int i = SkipEmpty(tokens, 0);
                        switch (l)
                        {
                            case 0:
                                if (i == -1 || tokens[i].StartsWith("//"))
                                {
                                    continue;
                                }
                                if (tokens[i] == "lang")
                                {
                                    WithoutLang = false;
                                    l++;
                                }
                                else if (tokens[i] == "Language")
                                {
                                    WithoutLang = true;
                                    l = 3;
                                }
                                break;
                            case 1:
                                if (i == -1 || tokens[i].StartsWith("//"))
                                {
                                    continue;
                                }
                                if (tokens[i] == "{")
                                {
                                    l++;
                                }
                                break;
                            case 2:
                                if (i == -1 || tokens[i].StartsWith("//"))
                                {
                                    continue;
                                }
                                if (tokens[i] == "Language")
                                {
                                    l++;
                                }
                                break;
                            case 3:
                                if (i == -1 || tokens[i].StartsWith("//"))
                                {
                                    continue;
                                }
                                if (tokens[i] == "Tokens")
                                {
                                    l++;
                                }
                                break;
                            case 4:
                                if (i == -1 || tokens[i].StartsWith("//"))
                                {
                                    continue;
                                }
                                if (tokens[i] == "{")
                                {
                                    l++;
                                }
                                break;
                            case 5:
                                LocalizationData cd;
                                if (i == -1 || tokens[i].StartsWith("//"))
                                {
                                    cd = new LocalizationData();
                                    if (tokens.Length > 0)
                                    {
                                        cd.DelimeterID = line;
                                    }
                                    data.Add(cd);
                                    continue;
                                }
                                if (tokens[i] == "}")
                                {
                                    l++;
                                    continue;
                                }

                                // All work
                                bool eng = tokens[i].StartsWith("[english]");
                                int j = i + 2;
                                string s = tokens[i];
                                if (eng)
                                {
                                    s = s.Remove(0, 9);
                                }
                                cd = (from d in data
                                      where d.ID == s
                                      select d).SingleOrDefault();
                                if (cd == null)
                                {
                                    cd = new LocalizationData();
                                    if (i > 0)
                                    {
                                        cd.DelimeterID = tokens[0];
                                    }
                                    cd.ID = s;
                                    data.Add(cd);
                                }
                                if (j < tokens.Length)
                                {
                                    if (tokens[j].IndexOf("\\n") >= 0)
                                    {
                                        cd.UseSlashN = true;
                                        tokens[j] = tokens[j].Replace("\\n", Environment.NewLine);
                                    }
                                    else
                                    {
                                        cd.UseSlashN = (ML ? false : true);
                                    }
                                    tokens[j] = tokens[j].Replace("\\\"", "\"");
                                    if (eng)
                                    {
                                        WithEnglishText = true;
                                        cd.English = tokens[j];
                                        cd.DelimeterEnglish = tokens[j - 1];
                                    }
                                    else
                                    {
                                        cd.Localized = tokens[j];
                                        cd.DelimeterLocalized = tokens[j - 1];
                                    }
                                }
                                break;
                            case 6:
                                if (i == -1 || tokens[i].StartsWith("//"))
                                {
                                    continue;
                                }
                                if (tokens[i] == "{")
                                {
                                    l++;
                                }
                                break;
                        }
                    }
                }
            }
            //catch (System.Exception ex)
            //{

            //}
            return data;
        }

        public void WriteData(string local, List<LocalizationData> data)
        {
            string fileName = GetLocalFileName(local);
            string fileNameBak = fileName + ".bak";
            if (File.Exists(fileNameBak))
            {
                File.Delete(fileNameBak);
            }
            File.Move(fileName, fileNameBak);
            //local = local.Substring(0, 1).ToUpper() + local.Substring(1);
            using (StreamWriter sw = new StreamWriter(fileName, false, System.Text.Encoding.Unicode))
            {
                if (!WithoutLang)
                {
                    sw.WriteLine("\"lang\"");
                    sw.WriteLine("{");
                }
                sw.WriteLine(String.Format("\"Language\" \"{0}\" ", local));
                sw.WriteLine("\"Tokens\"");
                sw.WriteLine("{");
                foreach (var d in data)
                {
                    if (d.ID != null)
                    {
                        if (DontSaveNotLocalized && d.English == d.Localized)
                        {
                            continue;
                        }
                        string loc;
                        if (!d.UseSlashN)
                        {
                            loc = d.Localized;
                        }
                        else
                        {
                            loc = d.Localized.Replace(Environment.NewLine, "\\n");
                        }
                        loc = loc.Replace("\"", "\\\"");
                        string line = String.Format("{0}\"{1}\"{2}\"{3}\"", d.DelimeterID, d.ID, d.DelimeterLocalized, loc);
                        sw.WriteLine(line);
                        if (WithEnglishText && d.EnglishOld != null)
                        {
                            string eng;
                            if (!d.EnglishTextChanged)
                            {
                                eng = d.English;
                            }
                            else
                            {
                                eng = d.EnglishOld;
                            }
                            if (d.UseSlashN)
                            {
                                eng = eng.Replace(Environment.NewLine, "\\n");
                            }
                            eng = eng.Replace("\"", "\\\"");
                            line = String.Format("{0}\"[english]{1}\"{2}\"{3}\"", d.DelimeterID, d.ID, d.DelimeterEnglish, eng);
                            sw.WriteLine(line);
                        }
                    }
                    else
                    {
                        sw.WriteLine(d.DelimeterID);
                    }
                }
                sw.WriteLine("}");
                if (!WithoutLang)
                {
                    sw.WriteLine("}");
                }
                sw.Close();
            }
        }
    }
}

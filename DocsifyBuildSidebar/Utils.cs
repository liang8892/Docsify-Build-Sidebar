using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Spectre.Console;

namespace DocsifyBuildSidebar
{
    public static class Utils
    {
        /// <summary>
        /// 判断是否是文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsFile(string path)
        {
            return File.Exists(path);
        }

        /// <summary>
        /// 判断是否是文件夹
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsDir(string path)
        {
            return Directory.Exists(path);
        }

        /// <summary>
        /// 生成markdown中的层级 空格
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static string GenerateSpace(int n)
        {
            var res = "";
            while (n-- > 0)
            {
                res += "  ";
            }

            return res;
        }

        /// <summary>
        /// 替换空格为%20
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string ReplaceSpace(string data)
        {
            return Regex.Replace(data, @"/\s{1,1}/g", "%20");
        }

        /// <summary>
        /// 获取不包含扩展名的 文件名
        /// </summary>
        /// <param name="file">文件对象</param>
        /// <returns></returns>
        public static string GetFileNameWithoutExtension(this FileInfo file)
        {
            return file.Name.Replace(file.Extension, "");
        }

        /// <summary>
        /// 获取文件的相对目录 (扩展方法)
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string GetFileRelativePath(this FileInfo file)
        {
            return file.FullName.Replace(GetCurrentDirectory(), "").TrimStart('\\').Replace("\\", "/");
        }

        /// <summary>
        /// 获取文件夹的相对目录 (扩展方法)
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string GetDirRelativePath(this DirectoryInfo dir)
        {
            string path = dir.FullName.Replace(GetCurrentDirectory(), "").TrimStart('\\');
            if (!string.IsNullOrWhiteSpace(path))
            {
                path += path[^1] == '\\' ? "" : "\\"; // 补齐地址末尾的 "\"
                path = path.Replace("\\", "/");
            }
            return path;
        }

        /// <summary>
        /// 获取当前运行目录
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentDirectory()
        {
            return System.Environment.CurrentDirectory;
        }

        /// <summary>
        /// 设置运行目录
        /// </summary>
        /// <param name="homePath"></param>
        /// <returns></returns>
        public static bool SetCurrentDirectory(string homePath)
        {
            System.Environment.CurrentDirectory = homePath;
            return System.Environment.CurrentDirectory == homePath;
        }

        /// <summary>
        /// 控制台日志
        /// </summary>
        /// <param name="message"></param>
        public static void WriteLogMessage(string message)
        {
            AnsiConsole.MarkupLine("[grey]LOG:[/]{0}", Markup.Escape(message));
        }

        /// <summary>
        /// 输出分割线
        /// </summary>
        public static void WriteDivider()
        {
            Console.WriteLine("-----------------------------");
        }

        /// <summary>
        /// 显示logo
        /// </summary>
        public static void ShowLogo()
        {

            //Console.SetWindowSize(150, Console.WindowHeight);

            var logo = new FigletText("build sidebar")
                .LeftAligned()
                .Color(Color.Yellow);
            AnsiConsole.Render(logo);

            var rule = new Rule("[red]build sidebar for c#[/]");
            rule.Alignment = Justify.Center;
            rule.Style = Style.Parse("red dim");

            AnsiConsole.Render(rule);
        }


        #region 生成图片md

        public static async Task CreateMDFileAsync(string rootPath, JsonConfigHelper configHelper)
        {
            var imageExtensions = configHelper.GetValue<List<string>>("ImageFileType");
            //循环查找rootPath下的所有包含图片文件的文件夹
            var dirs = Directory.GetDirectories(rootPath, "*", SearchOption.AllDirectories);
            Dictionary<string, List<string>> imageDirs = new Dictionary<string, List<string>>();
            foreach (var item in dirs)
            {
                var files = Directory.GetFiles(item, "*.*", SearchOption.TopDirectoryOnly).ToList();
                files.RemoveAll(a => !imageExtensions.Contains(Path.GetExtension(a).ToLower()));
                if (files.Count > 0)
                    imageDirs[item] = files;
            }
            if (imageDirs.Count == 0)
                return;

            //在包含图片的文件夹下，创建一个与文件夹同名的md文件，若文件已存在直接使用新生成的文件替换现有文件
            foreach (var dir in imageDirs)
            {
                List<string> output = new List<string>();
                for (int i = 0; i < dir.Value.Count; i++)
                {
                    var file = dir.Value[i];
                    output.Add($"![]({file.Replace(dir.Key, "")})");
                    output.Add($"<p style=\"text-align: center; font-size:20px;\">" +
                        $"<a href=\"{file.Replace(rootPath, "")}\" download=\"{Path.GetFileName(file)}\">下载</a></p>");
                    output.Add("\n");
                }
                string mdName = Path.GetFileName(dir.Key) + ".md";
                string mdPath = Path.Combine(dir.Key, mdName);
                await File.WriteAllLinesAsync(mdPath, output);
            }            
        }

        

        #endregion
    }
}
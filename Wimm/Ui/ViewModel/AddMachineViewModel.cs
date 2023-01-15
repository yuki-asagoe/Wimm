using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Wimm.Model.Control;
using Wimm.Model.Generator;

namespace Wimm.Ui.ViewModel
{
   public class AddMachineViewModel:DependencyObject
    {
        public static DependencyProperty FileNameProperty
            = DependencyProperty.Register("FileName", typeof(string), typeof(AddMachineViewModel));
        public string FileName
        {
            get { return (string)GetValue(FileNameProperty); }
            set { SetValue(FileNameProperty, value); }
        }
        public async Task<(string?,DirectoryInfo?)> AddMachine()
        {
            var file = new FileInfo(FileName);
            return await Task.Run(
                () =>
                {
                    DirectoryInfo? result=null;
                    string? errorText=null;
                    try
                    {
                        result=MachineFolderGenerator.CreateMachineDirectoryFrom(file);
                    }
                    catch(Exception e)//お行儀が悪いのはわかってるけど
                    {
                        errorText = $"{e.GetType().Name}:{e.Message}";
                    }
                    if(result is null && errorText is null)
                    {
                        errorText = "不明なエラーが発生しました。Dllファイルや既存のマシンディレクトリを確認してください。";
                    }
                    return (errorText, result);
                }
            );
        }  
    }
}

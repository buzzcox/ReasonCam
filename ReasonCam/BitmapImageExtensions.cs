using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace Xyzzer.AsyncUI
{
    public static class BitmapImageExtensions
    {
        public async static Task<ExceptionRoutedEventArgs> WaitForLoadedAsync(this BitmapImage bitmapImage)
        {
            var tcs = new TaskCompletionSource<ExceptionRoutedEventArgs>();

            // Need to set it to noll so that the compiler does not
            // complain about use of unassigned local variable.
            RoutedEventHandler reh = null;
            ExceptionRoutedEventHandler ereh = null;

            reh = (s, e) =>
            {
                bitmapImage.ImageOpened -= reh;
                bitmapImage.ImageFailed -= ereh;
                tcs.SetResult(null);
            };


            ereh = (s, e) =>
            {
                bitmapImage.ImageOpened -= reh;
                bitmapImage.ImageFailed -= ereh;
                tcs.SetResult(e);
            };

            bitmapImage.ImageOpened += reh;
            bitmapImage.ImageFailed += ereh;

            return await tcs.Task;
        }
    }
}
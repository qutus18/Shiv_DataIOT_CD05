using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SHIV_Data_Weigh.Commands
{
    public static class CustomCommands
    {
        public static RoutedUICommand Exit = new RoutedUICommand(
            "_Exit", "Exit", typeof(CustomCommands), // Name: Exit
            new InputGestureCollection
            {
                new KeyGesture(Key.F4, ModifierKeys.Alt)
            });
        public static RoutedUICommand StartSylvac = new RoutedUICommand(
            "_StartSylvac", "StartSylvac", typeof(CustomCommands), // Name: Exit
            new InputGestureCollection
            {
                new KeyGesture(Key.F7, ModifierKeys.None)
            });
        public static RoutedUICommand StopSylvac = new RoutedUICommand(
            "_StopSylvac", "StopSylvac", typeof(CustomCommands), // Name: Exit
            new InputGestureCollection
            {
                new KeyGesture(Key.F8, ModifierKeys.None)
            });
    }
 
}

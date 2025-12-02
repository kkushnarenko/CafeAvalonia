using CafeAvalonia.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CafeAvalonia.ViewModels
{
    public partial class ShiftDetailsViewModel : ReactiveObject
    {
        public Shift CurrentShift { get; }
        public string ShiftTime => $"{CurrentShift.DateStart:dd.MM HH:mm} - {CurrentShift.DateFinis:dd.MM HH:mm}";
        public int EmployeeCount => CurrentShift.Shiftassignments?.Count ?? 0;

        public ShiftDetailsViewModel(Shift shift, BdcafeContext dbContext)
        {
            CurrentShift = shift ?? throw new ArgumentNullException(nameof(shift));
            
        }
    }
}

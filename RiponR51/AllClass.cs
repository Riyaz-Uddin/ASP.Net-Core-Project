using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using System;

namespace RiponR51
{
    public class Doctor
    {
        [Key]
        public string Doctor_Code { get; set; }
        public string DoctorName { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
        public int Tel { get; set; }
        public string Designation { get; set; }
        public string Image { get; set; }
        public virtual ICollection<Patient> patients { get; set; }
    }

    public class Patient
    {
        [Key]
        public string Patient_Id { get; set; }

        [ForeignKey("doctors")]
        public string Doctor_Code { get; set; }
        public string PatientName { get; set; }
        public string Gender { get; set; }
        public string Number { get; set; }
        public Nullable<System.DateTime> Date
        { get; set; }
        public bool Active { get; set; }
        public virtual Doctor doctors { get; set; }
    }

    public class DoctorPatientsVM
    {
        [Required(ErrorMessage = "Please enter Doctor_Code")]
        public string Doctor_Code { get; set; }
        [Required(ErrorMessage = "Please enter Name")]
        [Display(Name = "DoctorName")]
        [MaxLength(50)]
        public string DoctorName { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
        public int Tel { get; set; }
        public string Designation { get; set; }
        public string Image { get; set; }
        public string Patient_Id { get; set; }
        public string PatientName { get; set; }
        public string Number { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; }
        public bool Active { get; set; }
    }
}

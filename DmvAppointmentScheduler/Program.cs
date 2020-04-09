/*
 Start output: Teller 9001238456 will work for 83718 minutes!

 Refactored Output: Teller 9001238456 will work for 29198 minutes! 
 */


using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace DmvAppointmentScheduler
{
    class Program
    {
        public static Random random = new Random();
        public static List<Appointment> appointmentList = new List<Appointment>();
        static void Main(string[] args)
        {
            CustomerList customers = ReadCustomerData();
            TellerList tellers = ReadTellerData();
            Calculation(customers, tellers);
            OutputTotalLengthToConsole();

        }
        //removed backslash to be able find path(Mac OS environment)
        private static CustomerList ReadCustomerData()
        {
            string fileName = "CustomerData.json";
            string path = Path.Combine(Environment.CurrentDirectory, @"InputData", fileName);
            string jsonString = File.ReadAllText(path);
            CustomerList customerData = JsonConvert.DeserializeObject<CustomerList>(jsonString);
            return customerData;

        }
        private static TellerList ReadTellerData()
        {
            string fileName = "TellerData.json";
            string path = Path.Combine(Environment.CurrentDirectory, @"InputData", fileName);
            string jsonString = File.ReadAllText(path);
            TellerList tellerData = JsonConvert.DeserializeObject<TellerList>(jsonString);
            return tellerData;

        }
        static void Calculation(CustomerList customers, TellerList tellers)
        {
            //each teller can have max appointments of this number
            int maxAppointmentsForTeller = customers.Customer.Count / tellers.Teller.Count;
            int tellerCount = 0;
            int randomCustomerTellerAssignment = 0;

            Appointment appointment = null;

            foreach (Customer customer in customers.Customer)
            {
                foreach (Teller teller in tellers.Teller)
                {
                    //in case if there's no matching service types
                    randomCustomerTellerAssignment = random.Next(0, tellers.Teller.Count);

                    //match all customers with tellers of same type
                    //break to next customer once a match occurs
                    if (customer.type == teller.specialtyType)
                    {
                        tellerCount = TellerAppointmentCount(teller);
                        //move to the next teller if teller reached max appointments
                        if(tellerCount >= maxAppointmentsForTeller)
                        {
                            continue;
                        }
                        appointment = new Appointment(customer, teller);
                        appointmentList.Add(appointment);
                        break;
                    }

                    tellerCount = TellerAppointmentCount(tellers.Teller[randomCustomerTellerAssignment]);

                    //move to next teller if random teller has reached max
                    if(tellerCount >= maxAppointmentsForTeller)
                    {
                        continue;
                    }
                    appointment = new Appointment(customer, tellers.Teller[randomCustomerTellerAssignment]);
                    appointmentList.Add(appointment);
                    break;
                }
            }

            CustomersWithoutAppointments(customers, tellers);
            AppointmentPerCustomerValidation(customers);
        }

        static int TellerAppointmentCount(Teller teller) {
            int tellerCount = 0;

            foreach(Appointment a in appointmentList) {
                if(teller.id == a.teller.id)
                {
                    tellerCount++;
                }
            }

            return tellerCount;
        }

        //give appointments to remaining customers
        static void CustomersWithoutAppointments(CustomerList customers, TellerList tellers)
        {
            List<Customer> customersWithoutAppointments = new List<Customer>();
            
            foreach(Customer customer in customers.Customer)
            {
                bool customerHasAppointment = false;
                foreach (Appointment a in appointmentList)
                {
                    if(customer.Id == a.customer.Id) {
                        //customer has appointment
                        customerHasAppointment = true;
                    }

                }

                if(customerHasAppointment == false)
                {
                    customersWithoutAppointments.Add(customer);
                }

            }

            foreach(Customer customer in customersWithoutAppointments)
            {                
                int assignmentRandom = random.Next(0, tellers.Teller.Count);

                var appointment = new Appointment(customer, tellers.Teller[assignmentRandom]);
                appointmentList.Add(appointment);                
            }
        }

        static void AppointmentPerCustomerValidation(CustomerList customers) {
            if(customers.Customer.Count == appointmentList.Count) {
                Console.WriteLine("All customers were given an appointment.\n");
                Console.WriteLine("No. of Appointments: {0}", appointmentList.Count);
                Console.WriteLine("No. of Customers: {0}\n", customers.Customer.Count);
            } else {
                Console.WriteLine("There are missing appointments for customers");
            }
        }

        static void OutputTotalLengthToConsole()
        {
            var tellerAppointments =
                from appointment in appointmentList
                group appointment by appointment.teller into tellerGroup
                select new
                {
                    teller = tellerGroup.Key,
                    totalDuration = tellerGroup.Sum(x => x.duration),
                };
            var max = tellerAppointments.OrderBy(i => i.totalDuration).LastOrDefault();
            Console.WriteLine("Teller " + max.teller.id + " will work for " + max.totalDuration + " minutes!");
        }

    }
}

using System;
using System.Collections.Generic;
class Program
{
    static void Main()
    {
        var railwaySystem = new RailwayReservationSystem();
        Login login = new Login(railwaySystem);
        login.DoLogin();
    }
}
class RailwayReservationSystem
{
    private Passenger currentPassenger;
    private Reservation currentReservation;
    private readonly TrainManager trainManager;
    private readonly ReservationManager reservationManager;
    private Ticket currentTicket;
    private Ticket ticket;
    public RailwayReservationSystem()
    {
        trainManager = new TrainManager();
        reservationManager = new ReservationManager(trainManager);
        InitializeData();
    }
    private void InitializeData()
    {
        trainManager.AddTrain(new Train("T001", "Express Train", 100));
        trainManager.AddTrain(new Train("T002", "Local Train", 50));
    }
    public void Start()
    {
        while (true)
        {
            Console.WriteLine("Railway Ticket Reservation System");
            Console.WriteLine("1. View Trains");
            Console.WriteLine("2. Reservation");
            Console.WriteLine("3. View Ticket");
            Console.WriteLine("*. Exit");
            Console.Write("Select an option: ");
            string choice = Console.ReadLine();
            switch (choice)
            {
                case "1":
                    Console.WriteLine("\n************************************************************\n");
                    trainManager.DisplayTrains();
                    Console.WriteLine("\n************************************************************\n");
                    break;
                case "2":
                    ReservationMenu();
                    break;
                case "3":
                    Console.WriteLine("\n------------------------------------------------------------\n");
                    ViewTicket();
                    Console.WriteLine("\n------------------------------------------------------------\n");
                    break;
                case "*":
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("Invalid choice. Please try again.");
                    break;
            }
        }
    }
    private void ViewTicket()
    {
        if (currentTicket != null)
        {
            Console.WriteLine("Ticket Details:");
            currentTicket.DisplayTicketDetails();
        }
        else
        {
            Console.WriteLine("No tickets generated.");
        }
    }
    private void ReservationMenu()
    {
        while (true)
        {
            Console.WriteLine("Reservation Menu");
            Console.WriteLine("1. Make Reservation");
            Console.WriteLine("2. View Reservations");
            Console.WriteLine("3. Cancel Reservation");
            Console.WriteLine("*. Back to Main Menu");
            Console.Write("Select an option: ");
            string choice = Console.ReadLine();
            switch (choice)
            {
                case "1":
                    Console.WriteLine("\n----------------------------------------------------------------------------------------------------\n");
                    MakeReservation();
                    Console.WriteLine("\n----------------------------------------------------------------------------------------------------\n");
                    break;
                case "2":
                    Console.WriteLine("\n----------------------------------------------------------\n");
                    ViewReservations();
                    Console.WriteLine("\n----------------------------------------------------------\n");
                    break;
                case "3":
                    CancelReservation();
                    break;
                case "*":
                    return;
                default:
                    Console.WriteLine("Invalid choice. Please try again.");
                    break;
            }
        }
    }
    private void MakeReservation()
    {
        Console.WriteLine("Making Reservation...");
        Console.Write("Enter your PAX_id: ");
        int paxId = int.Parse(Console.ReadLine());
        Console.Write("Enter your name: ");
        string name = Console.ReadLine();
        Console.Write("Enter your age: ");
        int age = int.Parse(Console.ReadLine());
        Passenger passenger = new Passenger(paxId, name, age);
        passenger.Login();
        if (!passenger.HasValidPNR())
        {
            Console.WriteLine("Failed to generate PNR. Please try again later.");
            return;
        }
        trainManager.DisplayTrains();
        Console.Write("Enter train ID: ");
        string trainId = Console.ReadLine();
        Train selectedTrain = trainManager.GetTrain(trainId);
        if (selectedTrain == null)
        {
            Console.WriteLine("Invalid train ID. Please try again.");
            return;
        }
        Console.Write("Enter number of seats to reserve: ");
        int seatsToReserve = int.Parse(Console.ReadLine());
        if (reservationManager.ReserveSeats(selectedTrain, seatsToReserve))
        {
            Reservation reservation = new Reservation(passenger, selectedTrain, seatsToReserve);
            reservationManager.AddReservation(reservation);
            Console.WriteLine($"Reservation successful! PNR number: {passenger.PNRno}");
            int totalAmount = seatsToReserve * 250;
            Console.WriteLine("Reservation Details:");
            reservation.DisplayReservationDetails();
            Console.WriteLine($"Total Amount to be Paid: ${totalAmount}");
            // Prompt user for payment
            Console.Write("Do you want to proceed with payment? (Y/N): ");
            string proceedPayment = Console.ReadLine().Trim().ToUpper();
            if (proceedPayment == "Y" || proceedPayment == "y")
            {
                Console.WriteLine("\n1.UPI\t 2.Credit Card\t 3.Debit Card \n");
                string choice = Console.ReadLine();
                if(choice == "1") { Console.WriteLine("\nRedirecting to UPI"); }
                if (choice == "2") { Console.WriteLine("\nMaking Credit Card Payment"); }
                if (choice == "3") { Console.WriteLine("\nMaking Debit Card Payment\n"); }
                MakePayment(passenger, reservation);
            }
            else
            {
                Console.WriteLine("Payment not proceeded.");
            }
        }
        else
        {
            Console.WriteLine("No seats available at the moment. Please choose a smaller number of seats or select a different train.");
        }
    }
    private void ViewReservations()
    {
        Console.WriteLine("View Reservations");
        Console.Write("Enter your PNR number: ");
        string pnrNumber = Console.ReadLine();
        Passenger passenger = new Passenger(-1, "", -1);
        passenger.PNRno = pnrNumber;
        reservationManager.DisplayReservations(passenger);
    }
    private void CancelReservation()
    {
        Console.Write("Enter PNR number to cancel reservation: ");
        string pnrNumber = Console.ReadLine();
        Reservation reservation = reservationManager.GetReservation(pnrNumber);
        if (reservation == null)
        {
            Console.WriteLine("Reservation not found. Please enter a valid PNR number.");
        }
        else
        {
            reservationManager.CancelReservation(reservation);
            Console.WriteLine($"Reservation with PNR number {pnrNumber} canceled successfully.");
        }
    }
    private void MakePayment(Passenger passenger, Reservation reservation)
    {
        int amountPaid = reservation.Seats * 250;
        Console.WriteLine($"Payment for {reservation.Seats} seat(s) on train '{reservation.Train.Name}' successful! Amount Paid: ${amountPaid}");
        CreateTicket(passenger.Name, passenger.Age, passenger.PNRno, reservation, amountPaid);
    }
    private void CreateTicket(string passengerName, int age, string pnrNumber, Reservation reservation, int amountPaid)
    {
        currentTicket = new Ticket(passengerName, age, pnrNumber, reservation, amountPaid);
    }
}
class TrainManager
{
    private List<Train> trains = new List<Train>();
    public void AddTrain(Train train)
    {
        trains.Add(train);
    }
    public Train GetTrain(string trainId)
    {
        return trains.Find(t => t.Id == trainId);
    }
    public void DisplayTrains()
    {
        Console.WriteLine("Available Trains:");
        foreach (var train in trains)
        {
            train.DisplayTrainDetails();
        }
    }
}
class ReservationManager
{
    private List<Reservation> reservations = new List<Reservation>();
    private int reservationIdCounter = 1;
    private readonly TrainManager trainManager;
    public ReservationManager(TrainManager trainManager)
    {
        this.trainManager = trainManager;
    }
    public void AddReservation(Reservation reservation)
    {
        reservations.Add(reservation);
    }
    public Reservation GetReservation(string pnrNumber)
    {
        return reservations.Find(r => r.Passenger.PNRno == pnrNumber);
    }
    public void CancelReservation(Reservation reservation)
    {
        reservation.Train.AvailableSeats += reservation.Seats;
        reservations.Remove(reservation);
    }
    public bool ReserveSeats(Train train, int seats)
    {
        return seats <= train.AvailableSeats;
    }
    public void DisplayReservations(Passenger passenger)
    {
        Console.WriteLine("Current Reservations:");
        bool found = false;
        foreach (var reservation in reservations)
        {
            if (reservation.Passenger.PNRno == passenger.PNRno)
            {
                found = true;
                reservation.DisplayReservationDetails();
            }
        }
        if (!found)
        {
            Console.WriteLine("No reservations found for this passenger.");
        }
    }
}
class Passenger
{
    public int PAX_id { get; }
    public string Name { get; }
    public int Age { get; }
    public string PNRno { get; set; }
    public Passenger(int paxId, string name, int age)
    {
        PAX_id = paxId;
        Name = name;
        Age = age;
    }
    public bool HasValidPNR()
    {
        return !string.IsNullOrEmpty(PNRno);
    }
    public void Login()
    {
        PNRno = $"TXN{PAX_id}";
    }
}
class Train
{
    public string Id { get; }
    public string Name { get; }
    public int TotalSeats { get; }
    public int AvailableSeats { get; set; }
    public Train(string id, string name, int totalSeats)
    {
        Id = id;
        Name = name;
        TotalSeats = totalSeats;
        AvailableSeats = totalSeats;
    }
    public void DisplayTrainDetails()
    {
        Console.WriteLine($"{Id} - {Name} (Seats: {TotalSeats}, Available Seats: {AvailableSeats})");
    }
}
class Reservation
{
    public Passenger Passenger { get; }
    public Train Train { get; }
    public int Seats { get; }
    public Reservation(Passenger passenger, Train train, int seats)
    {
        Passenger = passenger;
        Train = train;
        Seats = seats;
    }
    public void DisplayReservationDetails()
    {
        Console.WriteLine($"PNR: {Passenger.PNRno}, Passenger: {Passenger.Name}, Train: {Train.Name}, Seats: {Seats}");
    }
}
class Ticket
{
    public string PassengerName { get; }
    public int Age { get; }
    public string PNRNumber { get; }
    public Reservation Reservation { get; }
    public int AmountPaid { get; }
    public Ticket(string passengerName, int age, string pnrNumber, Reservation reservation, int amountPaid)
    {
        PassengerName = passengerName;
        Age = age;
        PNRNumber = pnrNumber;
        Reservation = reservation;
        AmountPaid = amountPaid;
    }
    public void DisplayTicketDetails()
    {
        Console.WriteLine($"Passenger Name: {PassengerName}, Age: {Age}, PNR No: {PNRNumber}");
        Console.WriteLine($"Train: {Reservation.Train.Name}, Seats: {Reservation.Seats}, \nAmount Paid: {AmountPaid}");
    }
}
class Login
{
    private RailwayReservationSystem system;
    public Login(RailwayReservationSystem system)
    {
        this.system = system;
    }
    public void DoLogin()
    {
        Console.WriteLine("Welcome to Railway Ticket Reservation System");
        Console.Write("Enter your username: ");
        string username = Console.ReadLine();
        Console.Write("Enter your password: ");
        string password = Console.ReadLine();
        if (username == "railway" && password == "123456")
        {
            Console.WriteLine("Login successful!");
            system.Start();
        }
        else
        {
            Console.WriteLine("Invalid username or password. Please try again.");
            DoLogin();
        }
    }
}

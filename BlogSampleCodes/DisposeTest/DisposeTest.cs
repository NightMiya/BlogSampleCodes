namespace BlogSampleCodes.DisposeTest;

public static class DisposeTest
{
    public static void Run() {
        Console.WriteLine("Start All");
        RunInternal();
        RunInternalWithUsing();
        Console.WriteLine("End All");
    }
    
    private static void RunInternal() {
        Console.WriteLine("Start");
        var wheel = new Wheel(100);
        var car = new Car(wheel);
        car.Wheel = wheel;
        car.Dispose();
        wheel.Dispose();
        Console.WriteLine("End");
    }
    
    
    private static void RunInternalWithUsing() {
        Console.WriteLine("Start");
        using var wheel = new Wheel(100);
        using var car = new Car(wheel);
        car.Wheel = wheel;
        Console.WriteLine("End");
    }
    
    class Wheel : IDisposable{
        public int Radius { get; set; }
        public Wheel(int radius) {
            Radius = radius;
            Console.WriteLine("\tWheel is created.");
        }
        public void Dispose()  {
            Radius = 0;
            Console.WriteLine("\tWheel is destructed.");
        }
    }
    class Car : IDisposable{
        public Wheel Wheel { get; set; }

        public Car(Wheel wheel)
        {
            Wheel = wheel;
            Console.WriteLine("\tCar is created.");
        }
        public void Dispose() {
            Console.WriteLine("\tCar is destructed.");
        }
    }
}
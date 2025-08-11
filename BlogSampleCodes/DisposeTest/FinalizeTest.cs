namespace BlogSampleCodes.DisposeTest;

public static class FinalizeTest
{
    public static void Run() {
        RunInternal();
        GC.Collect();
        GC.WaitForFullGCComplete();
    }
    
    private static void RunInternal() {
        Console.WriteLine("Start");
        var wheel = new Wheel(100);
        var car = new Car(wheel);
        car.Wheel = wheel;
        wheel = null;
        car = null;
        GC.Collect();
        GC.WaitForFullGCComplete();
        Console.WriteLine("End");
    }
    
    class Wheel {
        public int Radius { get; set; }
        public Wheel(int radius) {
            Radius = radius;
            Console.WriteLine("\tWheel is created.");
        }
        ~Wheel() {
            Radius = 0;
            Console.WriteLine("\tWheel is destructed.");
        }
    }
    class Car {
        public Wheel Wheel { get; set; }

        public Car(Wheel wheel)
        {
            Wheel = wheel;
            Console.WriteLine("\tCar is created.");
        }
        ~Car() {
            Console.WriteLine("\tCar is destructed.");
        }
    }
}
package mainP;

import java.util.Scanner;

public class Main {

    public static void main(String[] args) {
        Scanner scanner = new Scanner(System.in);

        int choice;

        do {
            System.out.println("f(x) = x^3-x^2 + x - 40*sin(x)|x<20\nf(x)=x^4*cos(x)|x>=20\n\n1 -- вычислить значения f(x)\n2--найти максимум f(x)\n3--найти минимум\n4 -- Выйти");
            choice = scanner.nextInt();

            switch (choice) {
                case 1:
                    printValues(scanner);
                    break;

                case 2:
                    findMaximum(scanner);
                    break;
                case 3:
                    findMinimum(scanner);
                    break;
                case 4:
                    System.out.println("Выход");
                    break;

                default:
                    System.out.println("Ошибка команды");
            }

        } while (choice != 4);

        scanner.close();
    }

    public static double calculate(double x) {
        if(x<20)  return x * x * x - x * x + x - 20 - 40*Math.sin(x);
        else return x * x * x * x * Math.cos(x);
    }

    public static void printValues(Scanner scanner) {
        System.out.println("Введите начало, конец и шаг");
        double start = scanner.nextDouble();
        double end = scanner.nextDouble();
        double step = scanner.nextDouble();

        while (step <= 0) {
            step = scanner.nextDouble();
        }

        for (double x = start; x <= end; x += step) {
            double result = calculate(x);
            System.out.println(result);
        }
    }

    public static void findMaximum(Scanner scanner) {
        System.out.println("Введите начало, конец и шаг");
        double start = scanner.nextDouble();
        double end = scanner.nextDouble();
        double step = scanner.nextDouble();
        double maximum = calculate(start);
        for(; start <= end; start+=step){
            double value = calculate(start);
            if(value > maximum) maximum = value;
        }
        System.out.println("maximum: " + maximum);
    }

    public static void findMinimum(Scanner scanner) {
        System.out.println("Введите начало, конец и шаг");
        double start = scanner.nextDouble();
        double end = scanner.nextDouble();
        double step = scanner.nextDouble();
        double minimum = calculate(start);
        for(; start <= end; start+=step){
            double value = calculate(start);
            if(value < minimum) minimum = value;
        }
        System.out.println("minimum: " + minimum);
    }
}


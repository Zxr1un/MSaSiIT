package mainP;
import java.util.Scanner;

public class Main{
    public static void main(String[] args){
        Scanner scanner = new Scanner(System.in);
        System.out.print("Введите первое число: ");
        int x = scanner.nextInt();
        System.out.print("Введите второе число: ");
        int y = scanner.nextInt();
        int value = calculate(x, y);
        if (value >= 10 && value <= 100)
        {
            System.out.println("Результат находится в диапазоне");
        }
        else
        {
            System.out.println("Результат вне диапазона");
        }
        for (int i = 0; i < 3; i++)
        {
            value++;
        }
        System.out.println("Итог: " + value);
    }

    public static int calculate(int a, int b) {
        int result = 0;
        if (a > b)
        {
            result = a - b;
        }
        else {
            result = a + b;
        }
        return result * 2;
    }
}

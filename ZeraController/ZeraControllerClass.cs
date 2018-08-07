using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;

namespace ZeraController
{
    public class ZeraControllerClass
    {
        string comPort = "COM1";

        static double Voltage1 = 230;
        static double Voltage2 = 230;
        static double Voltage3 = 230;


        static double angle1 = 0;
        static double angle2 = 240;
        static double angle3 = 120;


        static double current1 = 5;
        static double current2 = 5;
        static double current3 = 5;

        static double anglec1 = 0;
        static double anglec2 = 240;
        static double anglec3 = 120;


        static bool current1_on = false;
        static bool current2_on = false;
        static bool current3_on = false;

        static double frequency = 50.0;

        static bool dosage_on = false;
        static bool dosage_running = false;

        static int energia_dosage = 0;
        static int energia_ja_injectada = 0;

        static DateTime inicio = DateTime.Now;
        static DateTime fim = DateTime.Now;
        static DateTime atual = DateTime.Now;

        static string StateOfPowerSupply = "SUIAAAAAAAAA";


        static int stateofdosage = 1;

        string valor_porto = Console.ReadLine();
        static string ReadZeraString = "";

        public static string[] GetSerialPorts()
        {
            return SerialPort.GetPortNames();
        }

        static SerialPort porto;

        static bool stringComplete = false;


        public static void OpenPort(string serialPortName)
        {
            try
            {
                porto = new SerialPort(serialPortName);
                porto.BaudRate = 9600;
                porto.Open();
            }
            catch (Exception)
            {

                throw;
            }

        }





        public static void ZeraController()
        {




        }

        public static void InitializeZera()
        {
            Console.WriteLine("AV");
            porto.Write("AV\r");
            Console.WriteLine("AAV");
            porto.Write("AAV\r");
            while (ReadZera() != "AAVACK")
            {

            }
            Console.WriteLine("AAW");
            porto.Write("AAW\r");
            while (ReadZera() != "AAWACK")
            {

            }
            Console.WriteLine("AAU");
            porto.Write("AAU\r");
            while (ReadZera() != "AAUACK")
            {

            }
            Console.WriteLine("AAI");
            porto.Write("AAI\r");
            while (ReadZera() != "AAIACK")
            {

            }
            Console.WriteLine("AML");
            porto.Write("AML\r");
            while (ReadZera() != "AMLACK")
            {

            }
            Console.WriteLine("AST");
            porto.Write("AST\r");
            while (ReadZera() != "ASTACK")
            {

            }
            Console.WriteLine("APF60000");
            porto.Write("APF60000\r");
            while (ReadZera() != "APFACK")
            {

            }
            Console.WriteLine("ATI 1");
            porto.Write("ATI 1\r");
            while (ReadZera() != "ATIACK")
            {

            }
            Console.WriteLine("AAMMMA");
            porto.Write("AAMMMA\r");
            while (ReadZera() != "AAMACK")
            {

            }
            VoltageSupplyState(false);
            Thread.Sleep(5 * 1000);
            CurrentSupplyState(false, false, false);

            VoltageSupplyState(true);


        }
        //        d.ToString("000.00");
        //d.ToString("000.#");

        public static void VoltageSupplyState(bool on)
        {

            char[] array = StateOfPowerSupply.ToCharArray();

            if (on)
            {
                array[3] = 'E';
                array[4] = 'E';
                array[5] = 'E';
            }
            else
            {
                array[3] = 'A';
                array[4] = 'A';
                array[5] = 'A';
            }


            StateOfPowerSupply = new string(array);

            porto.Write(StateOfPowerSupply + "\r");
            Console.WriteLine(StateOfPowerSupply);
            while (ReadZera() != "SOKUI")
            {

            }
        }

        public static void CurrentSupplyState(bool on1, bool on2, bool on3)
        {
            char[] array = StateOfPowerSupply.ToCharArray();

            if (on1)
            {
                array[6] = 'P';
            }
            else
            {
                array[6] = 'A';
            }

            if (on2)
            {
                array[7] = 'P';
            }
            else
            {
                array[7] = 'A';
            }

            if (on3)
            {
                array[8] = 'P';
            }
            else
            {
                array[8] = 'A';
            }

            StateOfPowerSupply = new string(array);
            Console.WriteLine(StateOfPowerSupply);
            porto.Write(StateOfPowerSupply + "\r");
            while (ReadZera() != "SOKUI")
            {

            }

        }






        public static void Dosage(int Energy, double I1, double I2, double I3, double ANG1, double ANG2, double ANG3, double FREQ)
        {
            string aux;
            ChangeToActive();
            //ativar dosage
            aux = "S3CM3";
            porto.Write(aux + "\r");
            Console.WriteLine(aux);
            ReadZera();

            aux = "S3SA1";
            porto.Write(aux + "\r");
            Console.WriteLine(aux);
            ReadZera(); //SOK3SA1;1




            ChangeCurrent(I1, I2, I3, ANG1, ANG2, ANG3, FREQ);

            VoltageSupplyState(true);
            CurrentSupplyState(true, true, true);

            aux = "AST";
            porto.Write(aux + "\r");
            Console.WriteLine(aux);
            while (ReadZera() != "ASTACK")
            {

            }

            aux = "S3PS46;" + (Energy * GetConstanteValue(I1)).ToString();
            porto.Write(aux + "\r");
            Console.WriteLine(aux);
            ReadZera();//SOK3PS46

            //começar a injectar
            aux = "S3CM1";
            porto.Write(aux + "\r");
            Console.WriteLine(aux);
            ReadZera();//SOK3CM1

            string aux_state;
            //pedir estado
            while (true)
            {
                aux = "S3SA1";
                porto.Write(aux + "\r");
                Console.WriteLine(aux);
                aux_state = ReadZera().Substring(8, 1);//SOK3SA1;2

                aux = "AST";
                porto.Write(aux + "\r");
                Console.WriteLine(aux);
                while (ReadZera() != "ASTACK")
                {

                }
                //pedir energia
                aux = "S3MA5";
                porto.Write(aux + "\r");
                Console.WriteLine(aux);
                energia_ja_injectada = int.Parse(ReadZera().Substring(8)) / GetConstanteValue(I1);// SOK3MA5;116943

                if (aux_state == "2")
                {

                }
                else
                {
                    CurrentSupplyState(false, false, false);

                    //desactivar dosage
                    aux = "S3CM4";
                    porto.Write(aux + "\r");
                    Console.WriteLine(aux);
                    ReadZera();//SOK3CM4
                    break;
                }
            }
            ChangeToActive();

        }

        public static void DosageReactive(double Energy, double I1, double I2, double I3, double ANG1, double ANG2, double ANG3, double FREQ)
        {
            string aux;
            ChangeToReactive();


            //ativar dosage
            aux = "S3CM3";
            porto.Write(aux + "\r");
            Console.WriteLine(aux);
            ReadZera();

            aux = "S3SA1";
            porto.Write(aux + "\r");
            Console.WriteLine(aux);
            ReadZera(); //SOK3SA1;1




            ChangeCurrent(I1, I2, I3, ANG1, ANG2, ANG3, FREQ);


       
            VoltageSupplyState(true);
            CurrentSupplyState(true, true, true);

            aux = "AST";
            porto.Write(aux + "\r");
            Console.WriteLine(aux);
            while (ReadZera() != "ASTACK")
            {

            }

            //escrever energia

            aux = "S3PS46;" + (Energy * GetConstanteValue(I1)).ToString();
            porto.Write(aux + "\r");
            Console.WriteLine(aux);
            ReadZera();//SOK3PS46

            //começar a injectar
            aux = "S3CM1";
            porto.Write(aux + "\r");
            Console.WriteLine(aux);
            ReadZera();//SOK3CM1

            string aux_state;
            //pedir estado
            while (true)
            {
                aux = "S3SA1";
                porto.Write(aux + "\r");
                Console.WriteLine(aux);
                aux_state = ReadZera().Substring(8, 1);//SOK3SA1;2

                aux = "AST";
                porto.Write(aux + "\r");
                Console.WriteLine(aux);
                while (ReadZera() != "ASTACK")
                {

                }
                //pedir energia
                aux = "S3MA5";
                porto.Write(aux + "\r");
                Console.WriteLine(aux);
                energia_ja_injectada = int.Parse(ReadZera().Substring(8)) / GetConstanteValue(I1);// SOK3MA5;116943

                if (aux_state == "2")
                {

                }
                else
                {
                    CurrentSupplyState(false, false, false);

                    //desactivar dosage
                    aux = "S3CM4";
                    porto.Write(aux + "\r");
                    Console.WriteLine(aux);
                    ReadZera();//SOK3CM4
                    break;
                }
            }
            ChangeToActive();
        }

        static string ReadZera()
        {
            ReadZeraString = "";
            stringComplete = false;
            while (true)
            {
                string ola = "a";
                char inChar = (char)porto.ReadChar();
                if (inChar == '\r')
                {
                    stringComplete = true;
                    Console.WriteLine(ReadZeraString);
                    return ReadZeraString;
                }
                else
                {
                    ReadZeraString += inChar;
                }
            }
        }

        //    SUPAER230.000000.00S230.000120.00T230.000240.00\r
        //   SIPAAR020.000015.00S020.000135.00T020.000255.00\r


        public static void ChangeCurrent(double I1, double I2, double I3, double ANG1, double ANG2, double ANG3, double FREQ)
        {
            current1 = I1;
            current2 = I2;
            current3 = I3;

            anglec1 = ANG1;
            anglec2 = ANG2;
            anglec3 = ANG3;

            string aux = "SIPAA";
            aux += "R";
            aux += I1.ToString("000.000");
            aux += ANG1.ToString("000.00");
            aux += "S";
            aux += I2.ToString("000.000");
            aux += ANG2.ToString("000.00");
            aux += "T";
            aux += I3.ToString("000.000");
            aux += ANG3.ToString("000.00");

            porto.Write(aux + "\r");
            Console.WriteLine(aux);
            while (ReadZera() != "SOKIP")
            {

            }

        }

        public static void ChangeVoltage(double V1, double V2, double V3, double ANG1, double ANG2, double ANG3, double FREQ)
        {
            Voltage1 = V1;
            Voltage2 = V2;
            Voltage3 = V3;

            angle1 = ANG1;
            angle2 = ANG2;
            angle3 = ANG3;

            string aux = "SUPAE";
            aux += "R";
            aux += V1.ToString("000.000");
            aux += ANG1.ToString("000.00");
            aux += "S";
            aux += V2.ToString("000.000");
            aux += ANG2.ToString("000.00");
            aux += "T";
            aux += V3.ToString("000.000");
            aux += ANG3.ToString("000.00");

            porto.Write(aux + "\r");
            Console.WriteLine(aux);
            while (ReadZera() != "SOKUP")
            {

            }
        }


        public static void ChangeToReactive()
        {
            Console.WriteLine("AMT4WR");
            porto.Write("AMT4WR\r");
            while (ReadZera() != "AMTACK")
            {

            }

        }

        public static void ChangeToActive()
        {
            Console.WriteLine("AMT4WA");
            porto.Write("AMT4WA\r");
            while (ReadZera() != "AMTACK")
            {

            }

        }

        private static int GetConstanteValue(double currentValue)
        {
            double currentNominalValue=0.1;


            if (currentValue>0.12)
            {
                currentNominalValue = 0.2;
            }
            if (currentValue > 0.24)
            {
                currentNominalValue = 0.5;
            }
            if (currentValue > 0.6)
            {
                currentNominalValue = 1;
            }
            if (currentValue > 1.2)
            {
                currentNominalValue = 2;
            }
            if (currentValue > 2.4)
            {
                currentNominalValue = 5;
            }
            if (currentValue > 6)
            {
                currentNominalValue = 10;
            }
            if (currentValue > 12)
            {
                currentNominalValue = 20;
            }
            if (currentValue > 24)
            {
                currentNominalValue = 50;
            }
            if (currentValue > 60)
            {
                currentNominalValue = 100;
            }
            if (currentValue > 120)
            {
                currentNominalValue = 200;
            }

            return (int)((60000.0 * 3600.0) / (3 * 250 * currentNominalValue));
        }


    }
}

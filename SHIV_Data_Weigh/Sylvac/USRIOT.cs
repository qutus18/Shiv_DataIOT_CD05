using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace SHIV_Data_Weigh.Sylvac
{
    public class USRIOT
    {
        public byte[] DO1_ON; public byte[] DO2_ON; public byte[] DO3_ON; public byte[] DO4_ON;
        public byte[] DO1_OFF; public byte[] DO2_OFF; public byte[] DO3_OFF; public byte[] DO4_OFF;
        public byte[] DI_Short_circuiting;
        public byte[] DI_NoShort_circuiting;
        public byte[] QueryAllDO;
        public byte[] DI1isClick; public byte[] DI2isClick; public byte[] DI3isClick; public byte[] DI4isClick;
        //public byte[] DO1_OnStatus; public byte[] DO2_OnStatus; public byte[] DO3_OnStatus; public byte[] DO4_OnStatus;
        public byte[] QueryDI1; public byte[] QueryDI2; public byte[] QueryDI3; public byte[] QueryDI4;
        public int vl1;
        //public int StatusDO;
        public string Address;
        int Port = 28899;
        TcpClient SocketTCP;
        public Stream USRIOT_Stream;
        private Thread ReceiverTCPthread;
        private int oldVL1;

        //IO change event
        public delegate void EvenchangeIO(string info);
        public event EvenchangeIO OnchangeIOevent;
        /// <summary>
        /// ConnectTCP to USR-IO424T by wifi
        /// </summary>
        public void ConnectTCP()
        {
            SocketTCP = new TcpClient();
            SocketTCP.Connect(Address, Port);
            if (SocketTCP.Connected)
            {
                Console.WriteLine("Connected");
                USRIOT_Stream = SocketTCP.GetStream();
                ReceiverTCPthread = new Thread(ReadStatusInput);
                ReceiverTCPthread.IsBackground = true;
                ReceiverTCPthread.Start();
            }
            else
            {
                Console.WriteLine("Connect is false");
            }
        }
        /// <summary>
        /// Creat CRC16/Mosbus
        /// </summary>
        public class Crc16
        {
            private static ushort[] CrcTable = {
            0X0000, 0XC0C1, 0XC181, 0X0140, 0XC301, 0X03C0, 0X0280, 0XC241,
            0XC601, 0X06C0, 0X0780, 0XC741, 0X0500, 0XC5C1, 0XC481, 0X0440,
            0XCC01, 0X0CC0, 0X0D80, 0XCD41, 0X0F00, 0XCFC1, 0XCE81, 0X0E40,
            0X0A00, 0XCAC1, 0XCB81, 0X0B40, 0XC901, 0X09C0, 0X0880, 0XC841,
            0XD801, 0X18C0, 0X1980, 0XD941, 0X1B00, 0XDBC1, 0XDA81, 0X1A40,
            0X1E00, 0XDEC1, 0XDF81, 0X1F40, 0XDD01, 0X1DC0, 0X1C80, 0XDC41,
            0X1400, 0XD4C1, 0XD581, 0X1540, 0XD701, 0X17C0, 0X1680, 0XD641,
            0XD201, 0X12C0, 0X1380, 0XD341, 0X1100, 0XD1C1, 0XD081, 0X1040,
            0XF001, 0X30C0, 0X3180, 0XF141, 0X3300, 0XF3C1, 0XF281, 0X3240,
            0X3600, 0XF6C1, 0XF781, 0X3740, 0XF501, 0X35C0, 0X3480, 0XF441,
            0X3C00, 0XFCC1, 0XFD81, 0X3D40, 0XFF01, 0X3FC0, 0X3E80, 0XFE41,
            0XFA01, 0X3AC0, 0X3B80, 0XFB41, 0X3900, 0XF9C1, 0XF881, 0X3840,
            0X2800, 0XE8C1, 0XE981, 0X2940, 0XEB01, 0X2BC0, 0X2A80, 0XEA41,
            0XEE01, 0X2EC0, 0X2F80, 0XEF41, 0X2D00, 0XEDC1, 0XEC81, 0X2C40,
            0XE401, 0X24C0, 0X2580, 0XE541, 0X2700, 0XE7C1, 0XE681, 0X2640,
            0X2200, 0XE2C1, 0XE381, 0X2340, 0XE101, 0X21C0, 0X2080, 0XE041,
            0XA001, 0X60C0, 0X6180, 0XA141, 0X6300, 0XA3C1, 0XA281, 0X6240,
            0X6600, 0XA6C1, 0XA781, 0X6740, 0XA501, 0X65C0, 0X6480, 0XA441,
            0X6C00, 0XACC1, 0XAD81, 0X6D40, 0XAF01, 0X6FC0, 0X6E80, 0XAE41,
            0XAA01, 0X6AC0, 0X6B80, 0XAB41, 0X6900, 0XA9C1, 0XA881, 0X6840,
            0X7800, 0XB8C1, 0XB981, 0X7940, 0XBB01, 0X7BC0, 0X7A80, 0XBA41,
            0XBE01, 0X7EC0, 0X7F80, 0XBF41, 0X7D00, 0XBDC1, 0XBC81, 0X7C40,
            0XB401, 0X74C0, 0X7580, 0XB541, 0X7700, 0XB7C1, 0XB681, 0X7640,
            0X7200, 0XB2C1, 0XB381, 0X7340, 0XB101, 0X71C0, 0X7080, 0XB041,
            0X5000, 0X90C1, 0X9181, 0X5140, 0X9301, 0X53C0, 0X5280, 0X9241,
            0X9601, 0X56C0, 0X5780, 0X9741, 0X5500, 0X95C1, 0X9481, 0X5440,
            0X9C01, 0X5CC0, 0X5D80, 0X9D41, 0X5F00, 0X9FC1, 0X9E81, 0X5E40,
            0X5A00, 0X9AC1, 0X9B81, 0X5B40, 0X9901, 0X59C0, 0X5880, 0X9841,
            0X8801, 0X48C0, 0X4980, 0X8941, 0X4B00, 0X8BC1, 0X8A81, 0X4A40,
            0X4E00, 0X8EC1, 0X8F81, 0X4F40, 0X8D01, 0X4DC0, 0X4C80, 0X8C41,
            0X4400, 0X84C1, 0X8581, 0X4540, 0X8701, 0X47C0, 0X4680, 0X8641,
            0X8201, 0X42C0, 0X4380, 0X8341, 0X4100, 0X81C1, 0X8081, 0X4040 };

            public static ushort ComputeCrc(params byte[] buffer)
            {
                ushort crc = 0xFFFF;
                if (buffer == null) throw new ArgumentNullException();
                for (int i = 0; i < buffer.Length; ++i)
                {
                    crc = (ushort)((crc >> 8) ^ CrcTable[(crc ^ buffer[i]) & 0xff]);
                }
                return crc;
            }
            public static byte[] ComputeChecksumBytes(params byte[] buffer)
            {
                return BitConverter.GetBytes(ComputeCrc(buffer));
            }

        }
        /// <summary>
        /// Read Status Input
        /// </summary>
        public void ReadStatusInput()
        {
            //USR_DO_value();
            USR_DI_value();
            //USR_DI_Query();
            byte[] Data;
            int i = 1;
            while (i > 0)
            {

                USRIOT_Stream.Flush();
                vl1 = 0;
                Data = new byte[6];
                byte[] x = { 0x11, 0x02, 0x00, 0x20, 0x00, 0x04, 0x7A, 0x93 };
                USRIOT_Stream.Write(x, 0, x.Length);
                USRIOT_Stream.Read(Data, 0, Data.Length);
                if (DI1isClick.SequenceEqual(Data) == true) vl1 = 1;
                if (DI2isClick.SequenceEqual(Data) == true) vl1 = 2;
                if (DI3isClick.SequenceEqual(Data) == true) vl1 = 4;
                if (DI4isClick.SequenceEqual(Data) == true) vl1 = 8;
                if ((OnchangeIOevent != null)&&(oldVL1 != vl1)) OnchangeIOevent("Input" + vl1.ToString());
                oldVL1 = vl1;
            }
        }
        /// <summary>
        /// Set OutPut DO
        /// </summary>
        /// <param name="Output"></param>
        public void SetOutPut(int Output)
        {
            USR_DO_value();
            switch (Output)
            {
                case 1:
                    USRIOT_Stream.Write(DO1_ON, 0, DO1_ON.Length);
                    break;
                case 2:
                    USRIOT_Stream.Write(DO2_ON, 0, DO2_ON.Length);
                    break;
                case 3:
                    USRIOT_Stream.Write(DO3_ON, 0, DO3_ON.Length);
                    break;
                case 4:
                    USRIOT_Stream.Write(DO4_ON, 0, DO4_ON.Length);
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// Set Output 4 DO same time
        /// </summary>
        /// <param name="On1"></param>
        /// <param name="On2"></param>
        /// <param name="On3"></param>
        /// <param name="On4"></param>
        public void SetOutputAll(int On1, int On2, int On3, int On4)
        {
            USR_DO_value();
            switch (On1)
            {
                case 0:
                    USRIOT_Stream.Write(DO1_OFF, 0, DO1_OFF.Length);
                    break;
                case 1:
                    USRIOT_Stream.Write(DO1_ON, 0, DO1_ON.Length);
                    break;
                default:
                    break;
            }
            switch (On2)
            {
                case 0:
                    USRIOT_Stream.Write(DO2_OFF, 0, DO2_OFF.Length);
                    break;
                case 1:
                    USRIOT_Stream.Write(DO2_ON, 0, DO2_ON.Length);
                    break;
                default:
                    break;
            }
            switch (On3)
            {
                case 0:
                    USRIOT_Stream.Write(DO3_OFF, 0, DO3_OFF.Length);
                    break;
                case 1:
                    USRIOT_Stream.Write(DO3_ON, 0, DO3_ON.Length);
                    break;
                default:
                    break;
            }
            switch (On4)
            {
                case 0:
                    USRIOT_Stream.Write(DO4_OFF, 0, DO4_OFF.Length);
                    break;
                case 1:
                    USRIOT_Stream.Write(DO4_ON, 0, DO4_ON.Length);
                    break;
                default:
                    break;
            }

        }
        /// <summary>
        /// Reset OutPut DO
        /// </summary>
        /// <param name="RS_Output"></param>
        public void ReSetOutput(int RS_Output)
        {
            USR_DO_value();
            switch (RS_Output)
            {
                case 1:
                    USRIOT_Stream.Write(DO1_OFF, 0, DO1_OFF.Length);
                    break;
                case 2:
                    USRIOT_Stream.Write(DO2_OFF, 0, DO2_OFF.Length);
                    break;
                case 3:
                    USRIOT_Stream.Write(DO3_OFF, 0, DO3_OFF.Length);
                    break;
                case 4:
                    USRIOT_Stream.Write(DO4_OFF, 0, DO4_OFF.Length);
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// Read Status DO (ON or OFF)
        /// </summary>
        public void ReadStatusOutput()
        {
            USRIOT_Stream.Flush();
            byte[] ReadDO = new byte[6];
            QueryDO_Status();
            USRIOT_Stream.Write(QueryAllDO, 0, QueryAllDO.Length);
            USRIOT_Stream.Read(ReadDO, 0, ReadDO.Length);
            int Read = ReadDO[3];
            switch (Read)
            {
                case 1:
                    Console.WriteLine("D1 ON");
                    break;
                case 2:
                    Console.WriteLine("D2 ON");
                    break;
                case 4:
                    Console.WriteLine("D3 ON");
                    break;
                case 8:
                    Console.WriteLine("D4 ON");
                    break;
                case 0:
                    Console.WriteLine("No one button is ON");
                    break;
                default:
                    break;
            }

        }
        /// <summary>
        /// mã byte On và OFF cho 4 cổng OutPut
        /// </summary>
        public void USR_DO_value()
        {
            byte[] Address = { 0x11 }; byte[] Function = { 0x05 }; byte[] DataDO1 = { 0x00, 0x00 }; byte[] DataDO2 = { 0x00, 0x01 }; byte[] DataDO3 = { 0x00, 0x02 }; byte[] DataDO4 = { 0x00, 0x03 }; byte[] ON = { 0xFF, 0x00 }; byte[] OFF = { 0x00, 0x00 };
            //Data fomar RTU
            byte[] RTUFrameON1 = { Address[0], Function[0], DataDO1[0], DataDO1[1], ON[0], ON[1] };
            byte[] RTUFrameOFF1 = { Address[0], Function[0], DataDO1[0], DataDO1[1], OFF[0], OFF[1] };
            byte[] RTUFrameON2 = { Address[0], Function[0], DataDO2[0], DataDO2[1], ON[0], ON[1] };
            byte[] RTUFrameOFF2 = { Address[0], Function[0], DataDO2[0], DataDO2[1], OFF[0], OFF[1] };
            byte[] RTUFrameON3 = { Address[0], Function[0], DataDO3[0], DataDO3[1], ON[0], ON[1] };
            byte[] RTUFrameOFF3 = { Address[0], Function[0], DataDO3[0], DataDO3[1], OFF[0], OFF[1] };
            byte[] RTUFrameON4 = { Address[0], Function[0], DataDO4[0], DataDO4[1], ON[0], ON[1] };
            byte[] RTUFrameOFF4 = { Address[0], Function[0], DataDO4[0], DataDO4[1], OFF[0], OFF[1] };
            //------------------------------
            // calculator CRC 16/mosbus
            byte[] crcBuffeer1_ON = Crc16.ComputeChecksumBytes(RTUFrameON1);
            byte[] crcBuffeer2_ON = Crc16.ComputeChecksumBytes(RTUFrameON2);
            byte[] crcBuffeer3_ON = Crc16.ComputeChecksumBytes(RTUFrameON3);
            byte[] crcBuffeer4_ON = Crc16.ComputeChecksumBytes(RTUFrameON4);
            byte[] crcBuffeer1_OFF = Crc16.ComputeChecksumBytes(RTUFrameOFF1);
            byte[] crcBuffeer2_OFF = Crc16.ComputeChecksumBytes(RTUFrameOFF2);
            byte[] crcBuffeer3_OFF = Crc16.ComputeChecksumBytes(RTUFrameOFF3);
            byte[] crcBuffeer4_OFF = Crc16.ComputeChecksumBytes(RTUFrameOFF4);
            // byte ON and OFF value OUTPUT
            DO1_ON = new byte[] { Address[0], Function[0], DataDO1[0], DataDO1[1], ON[0], ON[1], crcBuffeer1_ON[0], crcBuffeer1_ON[1] };
            DO2_ON = new byte[] { Address[0], Function[0], DataDO2[0], DataDO2[1], ON[0], ON[1], crcBuffeer2_ON[0], crcBuffeer2_ON[1] };
            DO3_ON = new byte[] { Address[0], Function[0], DataDO3[0], DataDO3[1], ON[0], ON[1], crcBuffeer3_ON[0], crcBuffeer3_ON[1] };
            DO4_ON = new byte[] { Address[0], Function[0], DataDO4[0], DataDO4[1], ON[0], ON[1], crcBuffeer4_ON[0], crcBuffeer4_ON[1] };
            DO1_OFF = new byte[] { Address[0], Function[0], DataDO1[0], DataDO1[1], OFF[0], OFF[1], crcBuffeer1_OFF[0], crcBuffeer1_OFF[1] };
            DO2_OFF = new byte[] { Address[0], Function[0], DataDO2[0], DataDO2[1], OFF[0], OFF[1], crcBuffeer2_OFF[0], crcBuffeer2_OFF[1] };
            DO3_OFF = new byte[] { Address[0], Function[0], DataDO3[0], DataDO3[1], OFF[0], OFF[1], crcBuffeer3_OFF[0], crcBuffeer3_OFF[1] };
            DO4_OFF = new byte[] { Address[0], Function[0], DataDO4[0], DataDO4[1], OFF[0], OFF[1], crcBuffeer4_OFF[0], crcBuffeer4_OFF[1] };
        }
        /// <summary>
        /// mã byte 4 cổng DI khi ngắn mạch
        /// </summary>
        public void USR_DI_value()
        {
            DI_Short_circuiting = new byte[] { 0x11, 0x02, 0x01, 0x01, 0x64, 0x88 };
            DI_NoShort_circuiting = new byte[] { 0x11, 0x02, 0x01, 0x00, 0xA5, 0x48 };
            byte[] DI1_Click = { 0x11, 0x02, 0x01, 0x01 }; byte[] CRC_DI1_Click = Crc16.ComputeChecksumBytes(DI1_Click);
            byte[] DI2_Click = { 0x11, 0x02, 0x01, 0x02 }; byte[] CRC_DI2_Click = Crc16.ComputeChecksumBytes(DI2_Click);
            byte[] DI3_Click = { 0x11, 0x02, 0x01, 0x04 }; byte[] CRC_DI3_Click = Crc16.ComputeChecksumBytes(DI3_Click);
            byte[] DI4_Click = { 0x11, 0x02, 0x01, 0x08 }; byte[] CRC_DI4_Click = Crc16.ComputeChecksumBytes(DI4_Click);
            DI1isClick = new byte[] { 0x11, 0x02, 0x01, 0x01, CRC_DI1_Click[0], CRC_DI1_Click[1] };
            DI2isClick = new byte[] { 0x11, 0x02, 0x01, 0x02, CRC_DI2_Click[0], CRC_DI2_Click[1] };
            DI3isClick = new byte[] { 0x11, 0x02, 0x01, 0x04, CRC_DI3_Click[0], CRC_DI3_Click[1] };
            DI4isClick = new byte[] { 0x11, 0x02, 0x01, 0x08, CRC_DI4_Click[0], CRC_DI4_Click[1] };
        }
        /// <summary>
        /// Mã quét trạng thái DO
        /// </summary>
        private void QueryDO_Status()
        {
            byte[] QueryDO = { 0x11, 0x01, 0x00, 0x00, 0x00, 0x04 };
            byte[] CRCqueryDO = Crc16.ComputeChecksumBytes(QueryDO);
            QueryAllDO = new byte[] { 0x11, 0x01, 0x00, 0x00, 0x00, 0x04, CRCqueryDO[0], CRCqueryDO[1] };
        }
        /// <summary>
        /// USR_DI_Query
        /// </summary>
        private void USR_DI_Query()
        {
            byte[] Query1 = { 0x11, 0x002, 0x00, 0x20, 0x00, 0x01 };
            byte[] Query2 = { 0x11, 0x002, 0x00, 0x21, 0x00, 0x01 };
            byte[] Query3 = { 0x11, 0x002, 0x00, 0x22, 0x00, 0x01 };
            byte[] Query4 = { 0x11, 0x002, 0x00, 0x24, 0x00, 0x01 };
            //------------------------------------------------------
            byte[] CRCQuery1 = Crc16.ComputeChecksumBytes(Query1);
            byte[] CRCQuery2 = Crc16.ComputeChecksumBytes(Query2);
            byte[] CRCQuery3 = Crc16.ComputeChecksumBytes(Query3);
            byte[] CRCQuery4 = Crc16.ComputeChecksumBytes(Query4);
            //------------------------------------------------------
            QueryDI1 = new byte[] { 0x11, 0x002, 0x00, 0x20, 0x00, 0x01, CRCQuery1[0], CRCQuery1[1] };
            QueryDI2 = new byte[] { 0x11, 0x002, 0x00, 0x20, 0x00, 0x01, CRCQuery2[0], CRCQuery2[1] };
            QueryDI3 = new byte[] { 0x11, 0x002, 0x00, 0x20, 0x00, 0x01, CRCQuery3[0], CRCQuery3[1] };
            QueryDI4 = new byte[] { 0x11, 0x002, 0x00, 0x20, 0x00, 0x01, CRCQuery4[0], CRCQuery4[1] };
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Wimm.Machines.TpipForRasberryPi.Import
{
    /// <summary>
    /// TPJTwr_42.dll Binding generated by P/Invoke Interop Assistant
    /// </summary>
    public class TPJT4
    {

        [StructLayout(LayoutKind.Sequential)]
        public struct OUT_DT_STR
        {

            /// unsigned short
            public ushort d_out;

            /// short
            public short PWM_TPIP4;

            /// short[4]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4, ArraySubType = UnmanagedType.I2)]
            public short[] PWM;

            /// short[16]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16, ArraySubType = UnmanagedType.I2)]
            public short[] PWM2;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct INP_DT_STR
        {

            /// unsigned short
            public ushort b_ptn;

            /// unsigned short[8]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8, ArraySubType = UnmanagedType.U2)]
            public ushort[] AI;

            /// short[4]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4, ArraySubType = UnmanagedType.I2)]
            public short[] PI;

            /// unsigned short
            public ushort batt;

            /// short[2]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2, ArraySubType = UnmanagedType.I2)]
            public short[] PI2;

            /// unsigned short
            public ushort DI;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct CanMessage
        {

            /// unsigned char
            public byte flg;

            /// unsigned char
            public byte RTR;

            /// unsigned char
            public byte sz;

            /// unsigned char
            public byte stat;

            /// unsigned short
            public ushort STD_ID;

            /// unsigned char[8]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] data;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct I2CMessage
        {

            /// unsigned char
            public byte slave_id;

            /// unsigned char
            public byte nouse;

            /// unsigned int
            public uint sz;

            /// unsigned char[1024]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
            public byte data;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct INP_PI_DT_STR
        {

            /// unsigned int
            public uint PI01_z;

            /// int
            public int PI01_ab;

            /// unsigned int
            public uint PI02_z;

            /// int
            public int PI02_ab;

            /// unsigned int
            public uint PI03_z;

            /// int
            public int PI03_ab;

            /// unsigned int
            public uint PI04_z;

            /// int
            public int PI04_ab;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct INP_PI2_DT_STR
        {

            /// unsigned int
            public uint PI05_z;

            /// int
            public int PI05_ab;

            /// unsigned int
            public uint PI06_z;

            /// int
            public int PI06_ab;

            /// unsigned int
            public uint PI07_z;

            /// int
            public int PI07_ab;

            /// unsigned int
            public uint PI08_z;

            /// int
            public int PI08_ab;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct mctrl_gps_t
        {

            /// unsigned char
            public byte YY;

            /// unsigned char
            public byte MM;

            /// unsigned char
            public byte DD;

            /// unsigned char
            public byte reserv1;

            /// unsigned char
            public byte hh;

            /// unsigned char
            public byte mm;

            /// unsigned char
            public byte ss;

            /// unsigned char
            public byte reserv2;

            /// int
            public int la_deg;

            /// int
            public int lo_deg;

            /// unsigned char
            public byte GPS_qlty;

            /// unsigned char
            public byte sat_cnt;

            /// unsigned short
            public ushort HDOP;

            /// unsigned short
            public ushort speed;

            /// unsigned short
            public ushort course;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct mctrl_hd_str
        {

            /// unsigned char
            public byte ver;

            /// unsigned char
            public byte msg_no;

            /// unsigned char
            public byte d_id;

            /// unsigned char
            public byte stat;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct mctrl_dt_t
        {

            /// mctrl_hd_t->mctrl_hd_str
            public mctrl_hd_str hd;

            /// char[36]
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 36)]
            public string dt;
        }

        public partial class NativeMethods
        {

            /// Return Type: void
            ///host_ip: char*
            ///hwnd: HWND->HWND__*
            [DllImport("TPJTwr_42.dll", EntryPoint = "TPJT_init", CallingConvention = CallingConvention.StdCall)]
            public static extern void init([System.Runtime.InteropServices.InAttribute()][MarshalAs(UnmanagedType.LPStr)] string host_ip, System.IntPtr hwnd);


            /// Return Type: void
            ///host_ip: char*
            ///hwnd: HWND->HWND__*
            ///req: unsigned int
            [DllImport("TPJTwr_42.dll", EntryPoint = "TPJT_init_ex", CallingConvention = CallingConvention.StdCall)]
            public static extern void init_ex([System.Runtime.InteropServices.InAttribute()][MarshalAs(UnmanagedType.LPStr)] string host_ip, System.IntPtr hwnd, uint req);


            /// Return Type: void
            [DllImport("TPJTwr_42.dll", EntryPoint = "TPJT_close", CallingConvention = CallingConvention.StdCall)]
            public static extern void close();


            /// Return Type: int
            ///req: unsigned int
            ///wait_time: int
            [DllImport("TPJTwr_42.dll", EntryPoint = "TPJT_set_com_req", CallingConvention = CallingConvention.StdCall)]
            public static extern int set_com_req(uint req, int wait_time);


            /// Return Type: int
            [DllImport("TPJTwr_42.dll", EntryPoint = "TPJT_get_com_req", CallingConvention = CallingConvention.StdCall)]
            public static extern int get_com_req();


            /// Return Type: int
            [DllImport("TPJTwr_42.dll", EntryPoint = "TPJT_get_com_stat", CallingConvention = CallingConvention.StdCall)]
            public static extern int get_com_stat();


            /// Return Type: int
            ///buf: out_dt_t*
            ///buf_size: int
            [DllImport("TPJTwr_42.dll", EntryPoint = "TPJT_set_ctrl", CallingConvention = CallingConvention.StdCall)]
            public static extern int set_ctrl(ref OUT_DT_STR buf, int buf_size);


            /// Return Type: int
            ///buf: inp_dt_t*
            ///buf_size: int
            [DllImport("TPJTwr_42.dll", EntryPoint = "TPJT_get_sens", CallingConvention = CallingConvention.StdCall)]
            public static extern int get_sens(ref INP_DT_STR buf, int buf_size);

            [DllImport("TPJTwr_42.dll", EntryPoint = "TPJT_get_sens", CallingConvention = CallingConvention.StdCall)]
            public static extern int get_sens([Out]INP_DT_STR[] buf, int buf_size);


            /// Return Type: int
            ///speed: int
            [DllImport("TPJTwr_42.dll", EntryPoint = "TPJT_set_vspeed", CallingConvention = CallingConvention.StdCall)]
            public static extern int set_vspeed(int speed);


            /// Return Type: int
            [DllImport("TPJTwr_42.dll", EntryPoint = "TPJT_get_vspeed", CallingConvention = CallingConvention.StdCall)]
            public static extern int get_vspeed();


            /// Return Type: int
            ///type: int
            [DllImport("TPJTwr_42.dll", EntryPoint = "TPJT_set_video_inf", CallingConvention = CallingConvention.StdCall)]
            public static extern int set_video_inf(int type);


            /// Return Type: int
            ///no: int
            [DllImport("TPJTwr_42.dll", EntryPoint = "TPJT_set_cam_no", CallingConvention = CallingConvention.StdCall)]
            public static extern int set_cam_no(int no);


            /// Return Type: int
            [DllImport("TPJTwr_42.dll", EntryPoint = "TPJT_get_cam_no", CallingConvention = CallingConvention.StdCall)]
            public static extern int get_cam_no();


            /// Return Type: int
            ///fps: int
            [DllImport("TPJTwr_42.dll", EntryPoint = "TPJT_set_video_fps", CallingConvention = CallingConvention.StdCall)]
            public static extern int set_video_fps(int fps);


            /// Return Type: int
            ///buf: void*
            ///buf_size: int
            [DllImport("TPJTwr_42.dll", EntryPoint = "TPJT_get_jpeg", CallingConvention = CallingConvention.StdCall)]
            public static extern int get_jpeg(System.IntPtr buf, int buf_size);


            /// Return Type: void*
            ///wait_flg: int
            ///wait_time: int
            ///dwBytesRead: int*
            [DllImport("TPJTwr_42.dll", EntryPoint = "TPJT_get_jpeg_file", CallingConvention = CallingConvention.StdCall)]
            public static extern System.IntPtr get_jpeg_file(int wait_flg, int wait_time, ref int dwBytesRead);


            /// Return Type: int
            [DllImport("TPJTwr_42.dll", EntryPoint = "TPJT_free_jpeg_file", CallingConvention = CallingConvention.StdCall)]
            public static extern int free_jpeg_file();


            /// Return Type: unsigned int
            [DllImport("TPJTwr_42.dll", EntryPoint = "TPJT_get_jpeg_timestamp", CallingConvention = CallingConvention.StdCall)]
            public static extern uint get_jpeg_timestamp();


            /// Return Type: int
            [DllImport("TPJTwr_42.dll", EntryPoint = "TPJT_get_Wlink", CallingConvention = CallingConvention.StdCall)]
            public static extern int get_Wlink();


            /// Return Type: int
            [DllImport("TPJTwr_42.dll", EntryPoint = "TPJT_get_Clink", CallingConvention = CallingConvention.StdCall)]
            public static extern int get_Clink();


            /// Return Type: int
            [DllImport("TPJTwr_42.dll", EntryPoint = "TPJT_get_com_mode", CallingConvention = CallingConvention.StdCall)]
            public static extern int get_com_mode();


            /// Return Type: int
            [DllImport("TPJTwr_42.dll", EntryPoint = "TPJT_get_com_version", CallingConvention = CallingConvention.StdCall)]
            public static extern int get_com_version();


            /// Return Type: int
            ///no: int
            [DllImport("TPJTwr_42.dll", EntryPoint = "TPJT_get_board_status", CallingConvention = CallingConvention.StdCall)]
            public static extern int get_board_status(int no);


            /// Return Type: unsigned int
            [DllImport("TPJTwr_42.dll", EntryPoint = "TPJT_get_delaytime", CallingConvention = CallingConvention.StdCall)]
            public static extern uint get_delaytime();


            /// Return Type: unsigned int
            [DllImport("TPJTwr_42.dll", EntryPoint = "TPJT_get_staytime", CallingConvention = CallingConvention.StdCall)]
            public static extern uint get_staytime();


            /// Return Type: int
            ///no: int
            ///buf: mctrl_can_t*
            ///size: int
            [DllImport("TPJTwr_42.dll", EntryPoint = "TPJT_Send_CANdata", CallingConvention = CallingConvention.StdCall)]
            public static extern int Send_CANdata(int no, ref CanMessage buf, int size);


            /// Return Type: int
            ///no: int
            ///buf: mctrl_can_t*
            [DllImport("TPJTwr_42.dll", EntryPoint = "TPJT_Recv_CANdata", CallingConvention = CallingConvention.StdCall)]
            public static extern int Recv_CANdata(int no, ref CanMessage buf);


            /// Return Type: int
            ///no: int
            ///buf: char*
            ///slave_id: int
            ///size: int
            [DllImport("TPJTwr_42.dll", EntryPoint = "TPJT_Send_I2Cdata", CallingConvention = CallingConvention.StdCall)]
            public static extern int Send_I2Cdata(int no, byte[] buf, int slave_id, int size);


            /// Return Type: int
            ///no: int
            ///slave_id: int
            ///size: int
            [DllImport("TPJTwr_42.dll", EntryPoint = "TPJT_Req_Recv_I2Cdata", CallingConvention = CallingConvention.StdCall)]
            public static extern int Req_Recv_I2Cdata(int no, int slave_id, int size);


            /// Return Type: int
            ///no: int
            ///buf: char*
            ///slave_id: int*
            ///recv_size: int*
            [DllImport("TPJTwr_42.dll", EntryPoint = "TPJT_Recv_I2Cdata", CallingConvention = CallingConvention.StdCall)]
            public static extern int Recv_I2Cdata(int no, byte[] buf, ref int slave_id, ref int recv_size);


            /// Return Type: int
            ///no: int
            [DllImport("TPJTwr_42.dll", EntryPoint = "TPJT_Req_Recv_PIdata", CallingConvention = CallingConvention.StdCall)]
            public static extern int Req_Recv_PIdata(int no);


            /// Return Type: int
            ///no: int
            [DllImport("TPJTwr_42.dll", EntryPoint = "TPJT_Req_Reset_PIdata", CallingConvention = CallingConvention.StdCall)]
            public static extern int Req_Reset_PIdata(int no);


            /// Return Type: int
            ///no: int
            ///buf: inp_pi_dt_t*
            [DllImport("TPJTwr_42.dll", EntryPoint = "TPJT_Recv_PIdata", CallingConvention = CallingConvention.StdCall)]
            public static extern int Recv_PIdata(int no, ref INP_PI_DT_STR buf);


            /// Return Type: int
            ///no: int
            [DllImport("TPJTwr_42.dll", EntryPoint = "TPJT_Req_Recv_PI2data", CallingConvention = CallingConvention.StdCall)]
            public static extern int Req_Recv_PI2data(int no);


            /// Return Type: int
            ///no: int
            [DllImport("TPJTwr_42.dll", EntryPoint = "TPJT_Req_Reset_PI2data", CallingConvention = CallingConvention.StdCall)]
            public static extern int Req_Reset_PI2data(int no);


            /// Return Type: int
            ///no: int
            ///buf: inp_pi2_dt_t*
            [DllImport("TPJTwr_42.dll", EntryPoint = "TPJT_Recv_PI2data", CallingConvention = CallingConvention.StdCall)]
            public static extern int Recv_PI2data(int no, ref INP_PI2_DT_STR buf);


            /// Return Type: int
            ///no: int
            ///buf: mctrl_dt_t*
            ///size: int
            [DllImport("TPJTwr_42.dll", EntryPoint = "TPJT_Send_CMDdata", CallingConvention = CallingConvention.StdCall)]
            public static extern int Send_CMDdata(int no, ref mctrl_dt_t buf, int size);


            /// Return Type: int
            ///no: int
            ///buf: mctrl_dt_t*
            [DllImport("TPJTwr_42.dll", EntryPoint = "TPJT_Recv_RESdata", CallingConvention = CallingConvention.StdCall)]
            public static extern int Recv_RESdata(int no, ref mctrl_dt_t buf);


            /// Return Type: int
            ///no: int
            ///buf: mctrl_can_t*
            ///buf_size: int
            [DllImport("TPJTwr_42.dll", EntryPoint = "TPJT_Recv_CANdata_ex", CallingConvention = CallingConvention.StdCall)]
            public static extern int Recv_CANdata_ex(int no, ref CanMessage buf, int buf_size);


            /// Return Type: unsigned int
            [DllImport("TPJTwr_42.dll", EntryPoint = "TPJT_Get_dtime", CallingConvention = CallingConvention.StdCall)]
            public static extern uint Get_dtime();


            /// Return Type: unsigned int
            [DllImport("TPJTwr_42.dll", EntryPoint = "TPJT_Get_Staytime", CallingConvention = CallingConvention.StdCall)]
            public static extern uint Get_Staytime();


            /// Return Type: int
            [DllImport("TPJTwr_42.dll", EntryPoint = "TPJT_Get_Wlink", CallingConvention = CallingConvention.StdCall)]
            public static extern int Get_Wlink();


            /// Return Type: int
            [DllImport("TPJTwr_42.dll", EntryPoint = "TPJT_Get_Clink", CallingConvention = CallingConvention.StdCall)]
            public static extern int Get_Clink();


            /// Return Type: unsigned int
            [DllImport("TPJTwr_42.dll", EntryPoint = "TPJT_JF_get_stamp", CallingConvention = CallingConvention.StdCall)]
            public static extern uint JF_get_stamp();


            /// Return Type: int
            ///no: int
            [DllImport("TPJTwr_42.dll", EntryPoint = "TPJT_Get_stat", CallingConvention = CallingConvention.StdCall)]
            public static extern int Get_stat(int no);


            /// Return Type: int
            ///no: int
            [DllImport("TPJTwr_42.dll", EntryPoint = "TPJT_chg_camera", CallingConvention = CallingConvention.StdCall)]
            public static extern int chg_camera(int no);


            /// Return Type: int
            [DllImport("TPJTwr_42.dll", EntryPoint = "TPJT_get_com_ver", CallingConvention = CallingConvention.StdCall)]
            public static extern int get_com_ver();

        }

    }
}

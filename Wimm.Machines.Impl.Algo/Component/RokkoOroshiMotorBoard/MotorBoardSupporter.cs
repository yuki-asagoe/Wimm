using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Wimm.Machines.Tpip3.Import;

namespace Wimm.Machines.Impl.Algo.Component.RokkoOroshiMotorBoard
{
    public class MotorBoardSupporter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="transportInfo">転送に関するCanID</param>
        /// <param name="data">転送するデータ。要素数が8以下である必要があります。</param>
        /// <returns>エラー値。エラーがあればfalse。</returns>
        public static bool Send(CanID transportInfo, byte[] data,byte targetBoardNumber=0)
        {
            if (data.Length > 8) return false;
            TPJT3.CanMessage message = new();
            message.flg = 0;//send
            message.RTR = 0;//?
            message.sz = (byte)data.Length;//size
            message.stat = 0;//?
            message.STD_ID = transportInfo.Construct();
            var copiedData = new byte[8];
            data.CopyTo(copiedData, 0);
            message.data = copiedData;
            var error = TPJT3.NativeMethods.Send_CANdata(targetBoardNumber, ref message, data.Length);
            return error != 0;
        }
    }
    public struct CanID
    {
        public CanDataType MessageType { get; set; }
        public CanDestinationAddress SourceAddress { get; set; }
        public CanDestinationAddress DestinationAddress{ get; set; }
        /// <summary>
        /// CAN通信メッセージフレームのIDとして利用できる値を生成します。
        /// </summary>
        /// <returns>
        /// 生成した値。CAN通信規格に基づき11bitで表現されます
        /// *****|***|****|****
        /// 順に 空白|メッセージタイプ|転送元ID|転送先アドレス|
        /// </returns>
        public ushort Construct()
        {
            return (ushort)((byte)MessageType << 8 | (byte)SourceAddress << 4 | (byte)DestinationAddress);
        }
    }
    public enum CanDataType:byte
    {
        /// <summary>
        /// 非常時用
        /// </summary>
        Emergency=0,
        /// <summary>
        /// 通常指示
        /// </summary>
        Command,
        /// <summary>
        /// 返答
        /// </summary>
        Response,
        /// <summary>
        /// 最大で 256*7-1 バイト、らしい
        /// 何が?
        /// </summary>
        Data,
        /// <summary>
        /// 心音(全ての接続されたボードは毎秒このメッセージを送信します。)
        /// </summary>
        HeartBeat=7
    }
    public enum CanDestinationAddress:byte
    {
        BroadCast=0
    }
}

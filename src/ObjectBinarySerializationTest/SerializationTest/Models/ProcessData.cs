using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerializationTest.Models
{
    public record ProcessData
    {
        /// <summary>
        /// 占10bit, CPU（所有内核的总处理利用率），最后一位表示小数位，比如253表示25.3%
        /// </summary>
        public short Cpu { get; set; }

        /// <summary>
        /// 占10bit, 内存（进程占用的物理内存），最后一位表示小数位，比如253表示25.3%，值可根据基本信息计算
        /// </summary>
        public short Memory { get; set; }

        /// <summary>
        /// 占10bit, 磁盘（所有物理驱动器的总利用率），最后一位表示小数位，比如253表示25.3%，值可根据基本信息计算
        /// </summary>
        public short Disk { get; set; }

        /// <summary>
        /// 占10bit, 网络（当前主要网络上的网络利用率），最后一位表示小数位，比如253表示25.3%，值可根据基本信息计算
        /// </summary>
        public short Network { get; set; }

        /// <summary>
        /// 占10bit, GPU(所有GPU引擎的最高利用率)，最后一位表示小数位，比如253表示25.3
        /// </summary>
        public short Gpu { get; set; }

        /// <summary>
        /// 占1bit，GPU引擎，0：无，1：GPU 0 - 3D
        /// </summary>
        public byte GpuEngine { get; set; }

        /// <summary>
        /// 占3bit，电源使用情况（CPU、磁盘和GPU对功耗的影响），0：非常低，1：低，2：中，3：高，4：非常高
        /// </summary>
        public byte PowerUsage { get; set; }

        /// <summary>
        /// 占3bit，电源使用情况趋势（一段时间内CPU、磁盘和GPU对功耗的影响），0：非常低，1：低，2：中，3：高，4：非常高
        /// </summary>
        public byte PowerUsageTrend { get; set; }

        /// <summary>
        /// 占1bit，进程类型，0：应用，1：后台进程
        /// </summary>
        public byte Type { get; set; }

        /// <summary>
        /// 占1bit，进程状态，0：正常运行，1：效率模式，2：挂起
        /// </summary>
        public byte Status { get; set; }

        public byte[] Serialize()
        {
            var bytes = new byte[8];

            // Cpu 
            bytes[0] = (byte)(Cpu >> 2);
            bytes[1] = (byte)(((Cpu & 0x03) << 6) | (Cpu >> 4));

            // Memory 
            bytes[2] = (byte)(((Memory & 0x0F) << 4) | (Memory >> 6));

            // Disk  
            bytes[3] = (byte)(((Disk & 0x3F) << 2) | (Disk >> 8));

            // Network  
            bytes[4] = (byte)(Network & 0xFF);

            // Gpu  
            bytes[5] = (byte)(Gpu >> 2);
            bytes[6] = (byte)((Gpu & 0x03) << 6);

            return bytes;
        }

        public static ProcessData Deserialize(byte[] buffer)
        {
            return new ProcessData()
            {
                Cpu = (short)((buffer[0] << 2) | (buffer[1] >> 6)),
                Memory = (short)((buffer[1] & 0x3F) << 4 | (buffer[2] >> 4)),
                Disk = (short)((buffer[2] & 0x0F) << 6 | (buffer[3] >> 2)),
                Network = (short)((buffer[3] & 0x03) << 8 | buffer[4]),
                Gpu = (short)((buffer[5] << 2) | (buffer[6] >> 6))
            };
        }
    }
}
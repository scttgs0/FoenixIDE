﻿namespace FoenixIDE.Simulator.Devices
{
    public class VDMA : MemoryLocations.MemoryRAM
    {
        private MemoryLocations.MemoryRAM System;
        private MemoryLocations.MemoryRAM Video;

        public VDMA(int StartAddress, int Length) : base(StartAddress, Length)
        {
        }

        public void SetVideoRam(MemoryLocations.MemoryRAM vram)
        {
            Video = vram;
        }

        public void SetSystemRam(MemoryLocations.MemoryRAM vram)
        {
            System = vram;
        }


        public override void WriteByte(int Address, byte Value)
        {
            data[Address] = Value;
            // The only address that matters is the register
            // If the Enable and Transfer bits are set then do the transfer
            if ((Address == 0 || Address == 0x20) && (Value & 0x81) == 0x81)
            {
                bool isSystemSource = (Value & 0x10) != 0;
                bool isSystemDest = (Value & 0x20) != 0;
                bool isFillTransfer = (Value & 4) != 0;

                // Setup variables
                int sizeSrcX = isSystemSource ? ReadWord(0x28) : ReadWord(8); // Max 65535
                int sizeSrcY = isSystemSource ? ReadWord(0x2A) : ReadWord(0xA); // Max 65535
                int sizeDestX = isSystemDest ? ReadWord(0x28) : ReadWord(8); // Max 65535
                int sizeDestY = isSystemDest ? ReadWord(0x2A) : ReadWord(0xA); // Max 65535
                int srcStride = (isSystemSource ? ReadWord(0x2C) : ReadWord(0xC)) & 0xFFFE; // must be an event number
                int destStride = (isSystemDest ? ReadWord(0x2E) : ReadWord(0xE)) & 0xFFFE; // must be an even number
                // if stride is zero, read data linearly
                srcStride = srcStride == 0 ? sizeSrcX : srcStride;
                // if stride is zero, write data linearly
                destStride = destStride == 0 ? sizeSrcX : destStride;


                MemoryLocations.MemoryRAM srcMemory;
                int srcAddr;
                bool isSrcTransfer2D;
                // Check if the source is system or video
                if (isSystemSource)
                {
                    srcMemory = System;
                    srcAddr = ReadLong(0x22); // Address $AF:0422
                    isSrcTransfer2D = (ReadByte(0x20) & 2) != 0;
                }
                else
                {
                    srcMemory = Video;
                    srcAddr = ReadLong(2); // Address $AF:0402
                    isSrcTransfer2D = (ReadByte(0) & 2) != 0;
                }

                MemoryLocations.MemoryRAM destMemory;
                int destAddr;
                bool isDestTransfer2D;
                if (isSystemDest)
                {
                    destMemory = System;
                    destAddr = ReadLong(0x25); // Address $AF:0425
                    isDestTransfer2D = (ReadByte(0x20) & 2) != 0;
                }
                else
                {
                    destMemory = Video;
                    destAddr = ReadLong(5); // Address $af:0405
                    isDestTransfer2D = (ReadByte(0) & 2) != 0;
                }

                // Check for fill transfer
                if (isFillTransfer)
                {
                    // we're copying the same byte in all destination addresses
                    byte transferByte = ReadByte(1); // Address $AF:0401

                    // Linear or 2D
                    if (!isDestTransfer2D)
                    {
                        int size1DTransfer = isSystemDest ? ReadLong(0x28) : ReadLong(8); // Address $AF:0408 - maximum 4MB
                        if (Video != null)
                        {
                            for (int i = 0; i < size1DTransfer; i++)
                            {
                                destMemory.WriteByte(destAddr + i, transferByte);
                            }
                        }
                    }
                    else
                    {
                        for (int y = 0; y < sizeDestY; y++)
                        {
                            for (int x = 0; x < sizeDestX; x++)
                            {
                                destMemory.WriteByte(destAddr + x + y * destStride, transferByte);
                            }
                        }
                    }
                }
                else
                {
                    // Load source data in buffer
                    byte[] buffer;
                    if (!isSrcTransfer2D)
                    {
                        int size1DTransfer = isSystemSource ? ReadLong(0x28) : ReadLong(0x8); // Address $AF:0408 - maximum 4MB
                        buffer = new byte[size1DTransfer];
                        srcMemory.CopyIntoBuffer(srcAddr, buffer, 0, size1DTransfer);
                    }
                    else
                    {
                        buffer = new byte[sizeSrcX * sizeSrcY];
                        int ptr = 0;
                        for (int y = 0; y < sizeSrcY; y++)
                        {
                            for (int x = 0; x < sizeSrcX; x++)
                            {
                                byte data = srcMemory.ReadByte(srcAddr + x + y * srcStride);
                                buffer[ptr++] = data;
                            }
                        }
                    }

                    // Transfer data from memory to VRAM
                    if (!isDestTransfer2D)
                    {
                        if (destMemory != null)
                        {
                            destMemory.CopyBuffer(buffer, 0, destAddr, buffer.Length);
                        }

                    }
                    else
                    {
                        int ptr = 0;
                        for (int y = 0; y < sizeDestY; y++)
                        {
                            for (int x = 0; x < sizeDestX; x++)
                            {
                                byte data = buffer[ptr++];
                                destMemory.WriteByte(destAddr + x + y * destStride, data);
                            }
                        }
                    }
                }

                // Raise an interrupt
                if ((Value & 8) == 8)
                {

                }
            }
        }
    }
}

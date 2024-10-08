﻿using System;

namespace FoenixIDE.Processor
{
    public partial class CPU
    {
        /// <summary>
        /// Accumulator
        /// </summary>
        public RegisterAccumulator A = new();
        /// <summary>
        /// Data Bank Register
        /// </summary>
        public RegisterBankNumber DataBank = new();
        /// <summary>
        /// Direct Page Register
        /// </summary>
        public RegisterDirectPage DirectPage = new();
        /// <summary>
        /// Program Bank Register
        /// </summary>
        //public RegisterBankNumber ProgramBank = new RegisterBankNumber();
        /// <summary>
        /// Program Counter
        /// </summary>
        //public Register16 PC = new Register16();
        public int PC = 0;
        /// <summary>
        /// Processor Status Register
        /// </summary>
        public Flags Flags = new();
        /// <summary>
        /// Stack Pointer. The stack is always in the first 64KB page.
        /// </summary>
        public Register16 Stack = new();
        /// <summary>
        /// X Index Regiser
        /// </summary>
        public Register X = new();
        /// <summary>
        /// Y Index Register
        /// </summary>
        public Register Y = new();
        /// <summary>
        /// Wait state. When Wait is true, the CPU will not exeucte instructions. It
        /// will service the IRQ, NMI, and ABORT lines. A hardware interrupt is required 
        /// to restart the CPU.
        /// </summary>
        public bool Waiting;

        // Aliases for long names:
        /// <summary>
        /// Direct page
        /// </summary>
        public RegisterDirectPage D
        {
            get { return DirectPage; }
        }
        /// <summary>
        /// program banK register
        /// </summary>
        //public Register8 K
        //{
        //    get { return ProgramBank; }
        //}
        /// <summary>
        /// Processor status register (P)
        /// </summary>
        public Flags P
        {
            get { return this.Flags; }
        }
        /// <summary>
        /// Stack pointer (S)
        /// </summary>
        public Register16 S
        {
            get { return this.Stack; }
        }

        public TimeSpan CycleTime
        {
            get
            {
                return DateTime.Now - checkStartTime;
            }
        }

    }
}

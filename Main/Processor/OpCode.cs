using System;

namespace FoenixIDE.Processor
{
    public class OpCode
    {
        public byte Value;
        public string Mnemonic;
        public AddressModes AddressMode;
        public delegate void ExecuteDelegate(byte Instruction, AddressModes AddressMode, int Signature);
        public event ExecuteDelegate ExecuteOp;
        public int Length8Bit;
        public Register ActionRegister = null;

        public OpCode(byte Value, string Mnemonic, int Length8Bit, Register ActionRegister, AddressModes Mode, ExecuteDelegate newDelegate)
        {
            this.Value = Value;
            this.Length8Bit = Length8Bit;
            this.ActionRegister = ActionRegister;
            this.Mnemonic = Mnemonic;
            this.AddressMode = Mode;
            this.ExecuteOp += newDelegate;

            global::System.Diagnostics.Debug.WriteLine("public const int " + Mnemonic + "_" + Mode.ToString() + "=0x" + Value.ToString("X2") + ";");
        }

        public OpCode(byte Value, string Mnemonic, int Length, AddressModes Mode, ExecuteDelegate newDelegate)
        {
            this.Value = Value;
            this.Length8Bit = Length;
            this.Mnemonic = Mnemonic;
            this.AddressMode = Mode;
            this.ExecuteOp += newDelegate;

            global::System.Diagnostics.Debug.WriteLine("public const int " + Mnemonic + "_" + Mode.ToString() + "=0x" + Value.ToString("X2") + ";");
        }

        public void Execute(int SignatureBytes)
        {
            if (ExecuteOp == null)
                throw new NotImplementedException("Tried to execute " + this.Mnemonic + " but it is not implemented.");

            ExecuteOp(Value, AddressMode, SignatureBytes);
        }

        public int Length
        {
            get
            {
                if (ActionRegister != null && ActionRegister.Width == 2)
                    return Length8Bit + 1;

                return Length8Bit;
            }
        }

        public override string ToString()
        {
            return this.Mnemonic + " " + this.AddressMode.ToString();
        }

        public string ToString(int Signature)
        {
            string sig;
            if (this.Length == 3)
                sig = "$" + Signature.ToString("X4");
            else if (this.Length == 4)
                sig = "$" + Signature.ToString("X6");
            else
                sig = "$" + Signature.ToString("X2");

            string arg = this.AddressMode switch
            {
                AddressModes.Interrupt => sig,
                AddressModes.Immediate => "#" + sig,
                AddressModes.DirectPage or AddressModes.Absolute or AddressModes.AbsoluteLong => sig,
                AddressModes.DirectPageIndirect or AddressModes.JmpAbsoluteIndirect => "(" + sig + ")",
                AddressModes.DirectPageIndexedIndirectWithX or AddressModes.JmpAbsoluteIndexedIndirectWithX => "(" + sig + ",X)",
                AddressModes.DirectPageIndexedWithX or AddressModes.AbsoluteIndexedWithX or AddressModes.AbsoluteLongIndexedWithX => sig + ",X",
                AddressModes.DirectPageIndexedWithY or AddressModes.AbsoluteIndexedWithY or AddressModes.AbsoluteLongIndexedWithY => sig + ",Y",
                AddressModes.DirectPageIndirectIndexedWithY => "(" + sig + "),Y",
                AddressModes.DirectPageIndirectLong or AddressModes.JmpAbsoluteIndirectLong => "[" + sig + "]",
                AddressModes.DirectPageIndirectLongIndexedWithY => "[DP+" + sig + "],Y",
                AddressModes.ProgramCounterRelative or AddressModes.ProgramCounterRelativeLong => sig,
                //case AddressModes.StackAbsolute:
                //    arg = sig;
                //    break;
                AddressModes.StackDirectPageIndirect => sig,
                AddressModes.StackRelative => sig + ",S",
                AddressModes.StackRelativeIndirectIndexedWithY => "(" + sig + ",S),Y",
                AddressModes.StackProgramCounterRelativeLong => sig,
                _ => "",
            };

            return this.Mnemonic + " " + arg;
        }

    }
}

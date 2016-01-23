﻿using System;
using System.Linq;
using System.Text;
using Mono.Cecil.Cil;
using Mono.Cecil;

namespace Quart.Msiler.Lib
{
    public class ListingGenerator
    {
        private int _longestOpCode;

        private readonly MsilerOptions _options;

        public ListingGenerator() {
            this._options = Common.Instance.Options;
        }


        private string GetHeader(MethodEntity m) => $"Method: {m.MethodName}";

        private string GetOffset(Instruction i) {
            var f = (Common.Instance.Options.DecimalOffsets) ? "IL_{0:D4}" : "IL_{0:X4}";
            return String.Format(f, i.Offset);
        }

        private string GetOperand(Instruction i) {
            if (i.OpCode.Code == Code.Ldstr) {
                return @"""" + Helpers.ReplaceNewLineCharacters(i.Operand.ToString()) + @"""";
            }

            if (i.Operand == null) {
                return String.Empty;
            }

            if (i.Operand is Instruction) {
                return GetOffset((Instruction)i.Operand);
            }

            if (i.Operand is Instruction[]) {
                var operands = (Instruction[])i.Operand;
                var joined = String.Join(" | ", operands.Select(GetOffset));
                return $"[ {joined} ]";
            }

            if (this._options.SimplifyFunctionNames && (i.Operand is MethodReference)) {
                var m = (MethodReference)i.Operand;
                return $"{m.DeclaringType.FullName}.{m.Name}";
            }

            if (this._options.NumbersAsHex) {
                Int64 number;
                bool isNumeric = Int64.TryParse(i.Operand.ToString(), out number);
                if (isNumeric) {
                    return $"0x{number.ToString("X")}";
                }
            }

            return i.Operand.ToString();
        }

        public string GetOpCode(Instruction i) {
            var name = i.OpCode.Name;
            return (this._options.UpcaseOpCodes) ? name.ToUpper() : name;
        }

        private string InstructionToString(Instruction i, int longestOpcode) {
            var opcodePart = this._options.AlignListing
                ? GetOpCode(i).PadRight(longestOpcode + 1)
                : GetOpCode(i);
            return $"{GetOffset(i)} {opcodePart} {GetOperand(i)}";
        }

        public string Generate(MethodEntity method) {
            if (this._options.AlignListing) {
                this._longestOpCode = method.Instructions
                    .Select(i => i.OpCode.Name)
                    .Max(s => s.Length);
            }
            var sb = new StringBuilder();

            if (this._options.DisplayMethodNames) {
                sb.AppendLine(this.GetHeader(method));
                sb.AppendLine();
            }

            foreach (var instruction in method.Instructions) {
                if (this._options.IgnoreNops && instruction.OpCode.Code == Code.Nop) {
                    continue;
                }
                sb.AppendLine(InstructionToString(instruction, _longestOpCode));
            }
            return sb.ToString();
        }
    }
}

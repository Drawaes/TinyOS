// ------------------------------------------------------------------------------
// <copyright from='2002' to='2002' company='Scott Hanselman'>
//    Copyright (c) Scott Hanselman. All Rights Reserved.   
// </copyright> 
// ------------------------------------------------------------------------------
//
// Scott Hanselman's Tiny Academic Virtual CPU and OS
// Copyright (c) 2002, Scott Hanselman (scott@hanselman.com)
// All rights reserved.
// 
// A BSD License
// Redistribution and use in source and binary forms, with or without modification,
// are permitted provided that the following conditions are met:
// 
// Redistributions of source code must retain the above copyright notice, 
// this list of conditions and the following disclaimer. 
// Redistributions in binary form must reproduce the above copyright notice,
// this list of conditions and the following disclaimer in the documentation 
// and/or other materials provided with the distribution. 
// Neither the name of Scott Hanselman nor the names of its contributors
// may be used to endorse or promote products derived from this software without
// specific prior written permission. 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO,
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR 
// PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS 
// BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; 
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
// THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE 
// OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
using System;
using System.Collections;
using System.IO;
using System.Configuration;
using System.Collections.Generic;

namespace Hanselman.CST352
{
	/// <summary>
	/// Represents a Program (not a <see cref="Process"/>) on disk and the <see cref="Instruction"/>s it's comprised of.  
	/// Used as a utility class to load a <see cref="Program"/> off disk.
	/// </summary>
	public class Program
	{
		private List<Instruction> instructions = null;

        /// <summary>
        /// Public constructor for a Program
        /// </summary>
        /// <param name="instructionsParam">The collection of <see cref="Instruction"/> objects that make up this Program</param>
        public Program(IEnumerable<Instruction> instructionsParam) => instructions = new List<Instruction>(instructionsParam);

        /// <summary>
        /// Spins through the list of <see cref="Instruction"/> and creates an array of bytes 
        /// that is then copied into Memory by <see cref="OS.CreateProcess"/>
        /// </summary>
        /// <returns>Array of bytes representing the <see cref="Program"/> in memory</returns>
        unsafe public byte[] GetMemoryImage()
		{
            var listInstr = new List<byte>();

			foreach (var instr in instructions)
			{
				// Instructions are one byte
				listInstr.Add((byte)instr.OpCode);
				
				// Params are Four Bytes
				if (instr.Param1 != uint.MaxValue)
				{
					var paramBytes = CPU.UIntToBytes(instr.Param1);
					for (var i = 0; i < paramBytes.Length; i++)
                    {
                        listInstr.Add(paramBytes[i]);
                    }
                }
				
				if (instr.Param2 != uint.MaxValue)
				{
					var paramBytes = CPU.UIntToBytes(instr.Param2);
                    for (var i = 0; i < paramBytes.Length; i++)
                    {
                        listInstr.Add(paramBytes[i]);
                    }
                }
			}
			
			// Create and array of bytes and return the instructions in it
			return listInstr.ToArray();
		}

		/// <summary>
		/// Loads a Program from a file on disk.  For each line the Program, create an <see cref="Instruction"/>
		/// and pass the raw string to the Instructions's constructor.  The resulting list of <see cref="Instruction"/>
		/// is the Program
		/// </summary>
		/// <param name="fileName">file with code to load</param>
		/// <returns>a new loaded Program</returns>
		public static Program LoadProgram(string fileName) 
		{
            using (TextReader t = File.OpenText(fileName))
            {
                var instructions = new List<Instruction>();
                var strRawInstruction = t.ReadLine();
                while (strRawInstruction != null)
                {
                    instructions.Add(new Instruction(strRawInstruction));
                    strRawInstruction = t.ReadLine();
                }
                var p = new Program(instructions);
                t.Close();
                return p;
            }
		}

		/// <summary>
		/// For Debugging, pretty prints the Instructions that make up this Program
		/// </summary>
		public void DumpProgram()
		{
			if (bool.Parse(EntryPoint.Configuration["DumpProgram"]) == false)
				return;

			foreach (var i in instructions)
            {
                Console.WriteLine(i.ToString());
            }

            Console.WriteLine();
		}
	}
}

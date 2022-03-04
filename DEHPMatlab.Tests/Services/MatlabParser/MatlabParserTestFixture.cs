// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MatlabParserTestFixture.cs" company="RHEA System S.A.">
// Copyright (c) 2020-2022 RHEA System S.A.
// 
// Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate.
// 
// This file is part of DEHPMatlab
// 
// The DEHPMatlab is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 3 of the License, or (at your option) any later version.
// 
// The DEHPMatlab is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with this program; if not, write to the Free Software Foundation,
// Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301, USA.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace DEHPMatlab.Tests.Services.MatlabParser
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using DEHPMatlab.Services.MatlabParser;
    using DEHPMatlab.ViewModel.Row;

    using NUnit.Framework;

    [TestFixture]
    public class MatlabParserTestFixture
    {
        private IMatlabParser parser;

        [SetUp]
        public void Setup()
        {
            this.parser = new MatlabParser();
        }

        [Test]
        public void VerifyParsing()
        {
            string modifiedScriptFilePath;
            Assert.Throws<FileNotFoundException>(()=> this.parser.ParseMatlabScript("anInvalidPath", out modifiedScriptFilePath));

            List<MatlabWorkspaceRowViewModel> matlabWorkspaceViewModels = this.parser.ParseMatlabScript(Path.Combine(TestContext.CurrentContext.TestDirectory,
                "Resources", "GNC_Lab4.m"), out modifiedScriptFilePath);

            Assert.AreEqual(20, matlabWorkspaceViewModels.Count);
            Assert.IsTrue((double)matlabWorkspaceViewModels.First().ActualValue < 0);
            Assert.AreEqual(-6370, matlabWorkspaceViewModels.First().ActualValue);
            Assert.IsTrue(File.Exists(modifiedScriptFilePath));
            var lastMatlabVariable = matlabWorkspaceViewModels.Last();
            var arrayValue = (Array) lastMatlabVariable.ActualValue;
            Assert.AreEqual(4, arrayValue.GetLength(0));
            Assert.AreEqual(3, arrayValue.GetLength(1));
            File.Delete(modifiedScriptFilePath);
        }
    }
}

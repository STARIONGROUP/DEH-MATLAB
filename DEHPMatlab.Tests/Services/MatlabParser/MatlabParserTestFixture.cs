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

            List<MatlabWorkspaceRowViewModel> matlabWorkspaceViewModels = this.parser.ParseMatlabScript(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "GNC_Lab4.m"), 
                out modifiedScriptFilePath);

            Assert.AreEqual(6, matlabWorkspaceViewModels.Count);
            Assert.IsTrue(File.Exists(modifiedScriptFilePath));
            File.Delete(modifiedScriptFilePath);
        }
    }
}

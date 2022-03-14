// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TreeListSortingBehaviorTestFixture.cs" company="RHEA System S.A.">
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

namespace DEHPMatlab.Tests.Behaviors
{
    using DEHPMatlab.Behaviors;

    using DevExpress.Xpf.Grid;
    using DevExpress.Xpf.Grid.TreeList;

    using NUnit.Framework;

    public class TreeListSortingBehaviorTestFixture
    {
        private TreeListSortingBehavior sortingBehavior;
        private TreeListCustomColumnSortEventArgs argsNullsValue;
        private TreeListCustomColumnSortEventArgs argsNullValue1;
        private TreeListCustomColumnSortEventArgs argsNullValue2;
        private TreeListCustomColumnSortEventArgs argsSameType;
        private TreeListCustomColumnSortEventArgs argsNotSameType;
        private TreeListCustomColumnSortEventArgs argsTwoDecimalTypes;

        [SetUp]
        public void Setup()
        {
            this.sortingBehavior = new TreeListSortingBehavior();
            this.argsNullsValue = new TreeListCustomColumnSortEventArgs(new GridColumn(), new TreeListNode(), new TreeListNode(), null, null);
            this.argsNullValue1 = new TreeListCustomColumnSortEventArgs(new GridColumn(), new TreeListNode(), new TreeListNode(), 2, null);
            this.argsNullValue2 = new TreeListCustomColumnSortEventArgs(new GridColumn(), new TreeListNode(), new TreeListNode(), null, 2);
            this.argsSameType = new TreeListCustomColumnSortEventArgs(new GridColumn(), new TreeListNode(), new TreeListNode(), 0.2, 0.3);
            this.argsNotSameType = new TreeListCustomColumnSortEventArgs(new GridColumn(), new TreeListNode(), new TreeListNode(), "4E-05", (int)1);
            this.argsTwoDecimalTypes = new TreeListCustomColumnSortEventArgs(new GridColumn(), new TreeListNode(), new TreeListNode(), 0.2, (int)1);
        }

        [Test]
        public void VerifyTwoObjectComparision()
        {
            Assert.DoesNotThrow(() => this.sortingBehavior.AssociatedObject_CustomColumnSort(null, this.argsNullsValue));
            Assert.DoesNotThrow(() => this.sortingBehavior.AssociatedObject_CustomColumnSort(null, this.argsNullValue1));
            Assert.DoesNotThrow(() => this.sortingBehavior.AssociatedObject_CustomColumnSort(null, this.argsNullValue2));
            Assert.DoesNotThrow(() => this.sortingBehavior.AssociatedObject_CustomColumnSort(null, this.argsSameType));
            Assert.DoesNotThrow(() => this.sortingBehavior.AssociatedObject_CustomColumnSort(null, this.argsNotSameType));
            Assert.DoesNotThrow(() => this.sortingBehavior.AssociatedObject_CustomColumnSort(null, this.argsTwoDecimalTypes));
        }
    }
}

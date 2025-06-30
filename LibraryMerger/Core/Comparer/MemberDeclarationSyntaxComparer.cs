using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryMerger.Core.Comparer
{
    /// <summary>
    /// MemberDeclarationSyntaxを、メンバーの種類、その後に名前の順で比較します。
    /// </summary>
    public class MemberDeclarationSyntaxComparer : IComparer<MemberDeclarationSyntax>
    {
        public static readonly MemberDeclarationSyntaxComparer Default = new MemberDeclarationSyntaxComparer();

        // メンバーの種類ごとの優先順位を定義
        private enum MemberSortKind
        {
            Field = 0,
            Constructor = 1,
            Destructor = 2,
            Property = 3,
            Event = 4,
            Method = 5,
            Operator = 6,
            ConversionOperator = 7,
            NestedType = 8, // Class, Struct, Interface, Enum
            Other = 99
        }

        public int Compare(MemberDeclarationSyntax? x, MemberDeclarationSyntax? y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (y is null) return 1;
            if (x is null) return -1;

            // 1. メンバーの種類で比較
            var xKind = GetMemberSortKind(x);
            var yKind = GetMemberSortKind(y);

            int kindCompare = xKind.CompareTo(yKind);
            if (kindCompare != 0)
            {
                return kindCompare; // 種類が違えば、その順序で決定
            }

            // 2. 種類が同じなら、名前で比較
            string xName = GetMemberIdentifierText(x);
            string yName = GetMemberIdentifierText(y);

            return string.Compare(xName, yName, StringComparison.Ordinal);
        }

        private static MemberSortKind GetMemberSortKind(MemberDeclarationSyntax member)
        {
            return member.Kind() switch
            {
                SyntaxKind.FieldDeclaration => MemberSortKind.Field,
                SyntaxKind.ConstructorDeclaration => MemberSortKind.Constructor,
                SyntaxKind.DestructorDeclaration => MemberSortKind.Destructor,
                SyntaxKind.PropertyDeclaration => MemberSortKind.Property,
                SyntaxKind.EventFieldDeclaration or SyntaxKind.EventDeclaration => MemberSortKind.Event,
                SyntaxKind.MethodDeclaration => MemberSortKind.Method,
                SyntaxKind.OperatorDeclaration => MemberSortKind.Operator,
                SyntaxKind.ConversionOperatorDeclaration => MemberSortKind.ConversionOperator,
                SyntaxKind.ClassDeclaration or SyntaxKind.StructDeclaration or
                SyntaxKind.InterfaceDeclaration or SyntaxKind.EnumDeclaration => MemberSortKind.NestedType,
                _ => MemberSortKind.Other
            };
        }

        // 上記のSimpleMemberComparerからヘルパーメソッドを再利用
        private static string GetMemberIdentifierText(MemberDeclarationSyntax member)
        {
            // (SimpleMemberComparerと同じ実装)
            return member switch
            {
                MethodDeclarationSyntax m => m.Identifier.ValueText,
                PropertyDeclarationSyntax p => p.Identifier.ValueText,
                ConstructorDeclarationSyntax ctor => ctor.Identifier.ValueText,
                FieldDeclarationSyntax f => f.Declaration.Variables.FirstOrDefault()?.Identifier.ValueText ?? string.Empty,
                ClassDeclarationSyntax c => c.Identifier.ValueText,
                _ => string.Empty
            };
        }
    }
}

﻿Imports System.IO
Imports System.Text.RegularExpressions
'Imports System.Web.Script.Serialization
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports System.Linq


Module Module1

    Dim theList As New List(Of JToken)

    Public Function GetDataResults() As List(Of JToken)

        Dim UserRole As String = "Personnel Officer"
        Dim importFile As String = File.ReadAllText("C:\Users\sony\Documents\Visual Studio 2015\MyProjects\ReadJSONFile\ReadJSONFile\draft_6.json")
        Dim input_Role_BP As JObject = JObject.Parse(importFile) 'Parse the jsonString as a JObject
        Dim the_AABP As JToken = BP_Abs_Agg(input_Role_BP, UserRole)
        'Console.WriteLine(the_AABP) '\\\ AABP is obtained!!!
        Dim Extracted_DataArray As JArray = BPDataExtraction(the_AABP)
        'Console.WriteLine(Extracted_DataArray) '\\\ Extracted Data are obtained!!!



        TransJArrayToList(Extracted_DataArray)
        'For j = 0 To theList.Count - 1
        '    Console.WriteLine("j = {0}:  {1}", j, theList(j))
        'Next


        'Dim the_file As StreamWriter
        'the_file = My.Computer.FileSystem.OpenTextFileWriter("C:\Test.txt", True)
        'the_file.WriteLine(Extracted_DataArray)
        'the_file.Close()

        'Console.ReadLine()
        Return theList
    End Function

    Public Sub TransJArrayToList(ByVal inputJArray As JArray)
        Dim _List As List(Of JToken) = inputJArray.ToList
        For i = 0 To _List.Count - 1
            If _List(i).Type = 8 Then
                theList.Add(_List(i))
            Else ' theList(i).Type = 2 (JArray)
                TransJArrayToList(_List(i))
            End If

        Next
    End Sub



    Public Function BPDataExtraction(ByVal inputJToken As JToken) As JArray
        Dim resultJArray As New JArray

        Dim c As List(Of JToken) = inputJToken.Children().ToList
        Dim c1 As JProperty = c(0)
        If c1.Name = "Strict-order Seqential" Then
            resultJArray = UIDrvt_EleOprt_StrSeq_Task(inputJToken)
        ElseIf c1.Name = "Parallel_A" Then
            resultJArray = UIDrvt_EleOprt_ParallelA_Task(inputJToken)
        End If

        For i = 0 To resultJArray.Count - 1

            Dim d As List(Of JToken) = resultJArray(i).Children().ToList
            Dim d1 As JProperty = d(0)

            If d1.Name = "task" Then

                '//// retrieve the in-task data as JObject   From a Task JObject
                Dim a As List(Of JToken) = resultJArray(i).Children().ToList
                Dim a1 As JProperty = a(0)
                Dim a1_JObject As JObject = a1.Value
                Dim insideData_JObject As JObject = a1_JObject("insideData")
                '//// retrieve the in-task data as JObject

                resultJArray(i) = HandleInTaskDataObject(insideData_JObject)

            Else 'd1.Name = "Parallel_A"
                resultJArray(i) = BPDataExtraction(resultJArray(i))
            End If

        Next


        Return resultJArray
    End Function

    Public Function HandleInTaskDataObject(ByVal inputJToken As JToken) As JArray
        Dim resultJArray As New JArray

        Dim c As List(Of JToken) = inputJToken.Children().ToList
        Dim c1 As JProperty = c(0)
        Dim c1_ValueList As List(Of JToken) = c1.Value.ToList
        '\\\\ c1.Name and c1_ValueList are identified. 
        'Console.WriteLine(c1.Name)
        'Console.WriteLine(c1_ValueList)

        For i = 0 To c1_ValueList.Count - 1

            If c1.Name <> "Conditional" Then
                Dim d As List(Of JToken) = c1_ValueList(i).Children().ToList
                Dim d1 As JProperty = d(0)
                'Console.WriteLine(d1.Value)
                Dim f As List(Of JToken) = d1.Value.Children().ToList

                Dim f1 As JProperty = f(0)

                If f1.Name <> "attribute" Then
                    c1_ValueList(i) = HandleInTaskDataObject(d1.Value)
                End If
            Else   ' c1.Name = "Conditional"
                Dim d As List(Of JToken) = c1_ValueList(i).Children().ToList
                Dim d1 As JProperty = d(0)

                Dim f As List(Of JToken) = d1.Value.Children().ToList

                'Dim f1 As JProperty = f(0) '???????????????

                For j = 0 To f.Count - 1
                    Dim g As List(Of JToken) = f(j).Children().ToList
                    Dim m As JProperty = g(0)
                    Dim n As List(Of JToken) = m.Value.Children().ToList

                    Dim p As JProperty = n(0)
                    If p.Name <> "attribute" Then
                        c1_ValueList(i) = HandleInTaskDataObject(d1.Value)
                    End If
                Next
            End If
        Next





        Dim reboxedJObject As JObject = New JObject(
         New JProperty(c1.Name, New JArray(From ele In c1_ValueList
                                           Order By c1_ValueList.IndexOf(ele))))
        Dim reboxedJToken As JToken = reboxedJObject





        If c1.Name = "Strict-order Sequential" Then
            resultJArray = UIDrvt_EleOprt_StrSeq_Attr(reboxedJToken) 'inputJToken)
        ElseIf c1.Name = "Free-order Sequential" Then
            resultJArray = UIDrvt_EleOprt_FreeSeq_Attr(reboxedJToken) 'inputJToken)
        ElseIf c1.Name = "Conditional" Then
            resultJArray = UIDrvt_EleOprt_Conditional_Attr(reboxedJToken) 'inputJToken)
        End If

        Return resultJArray
    End Function

    Public Function UIDrvt_EleOprt_StrSeq_Task(ByVal inputJToken As JToken) As JToken
        Dim c As List(Of JToken) = inputJToken.Children().ToList
        Dim c1 As JProperty = c(0)
        Dim c1_ValueList As List(Of JToken) = c1.Value.ToList

        Dim _outputList As New JArray

        For Each childToken As JObject In c1_ValueList
            'Console.WriteLine(childToken)
            Dim d1 As List(Of JToken) = childToken.Children().ToList
            Dim d2 As JProperty = d1(0)

            If d2.Value.Type <> JTokenType.String Then  ' Not equals to
                'Console.WriteLine("Name: {0}", d2.Name)
                'Console.WriteLine("Value Type: {0}", d2.Value.Type)
                'Console.WriteLine("Value: {0}", d2.Value)
                _outputList.Add(childToken)
            End If
        Next

        'For k = 0 To _outputList.Count - 1
        '    Console.WriteLine("i = {0}: {1}", k, _outputList(k))
        'Next

        Return _outputList

    End Function

    Public Function UIDrvt_EleOprt_ParallelA_Task(ByVal inputJToken As JToken) As JToken
        Dim a As List(Of JToken) = inputJToken.Children().ToList
        Dim a1 As JProperty = a(0)
        Dim a2 As List(Of JToken) = a1.Value.ToList

        Dim outputJArray As New JArray

        For h = 0 To a2.Count - 1
            Dim a2_ As List(Of JToken) = a2(h).Children.ToList
            Dim a3 As JProperty = a2_(0)
            Dim a4 As List(Of JToken) = a3.Value.ToList

            For k = 0 To a4.Count - 1
                'Console.WriteLine("i = {0}: {1}", k, a4(k))
                Dim a5 As List(Of JToken) = a4(k).Children().ToList
                Dim a6 As JProperty = a5(0)
                If a6.Value.Type <> JTokenType.String Then
                    outputJArray.Add(a4(k))
                End If
            Next
        Next

        'Console.WriteLine(outputJArray)
        Return outputJArray

    End Function

    Public Function UIDrvt_EleOprt_StrSeq_Attr(ByVal inputJToken As JToken) As JToken

        Dim c As List(Of JToken) = inputJToken.Children().ToList
        Dim c1 As JProperty = c(0)
        Dim c1_ValueList As List(Of JToken) = c1.Value.ToList

        Dim _outputList As New JArray
        For i As Integer = 0 To c1_ValueList.Count - 1

            If c1_ValueList(i).Type = 1 Then
                Dim d As List(Of JToken) = c1_ValueList(i).Children().ToList
                Dim d1 As JProperty = d(0)
                'Console.WriteLine(d1.Value)
                Dim e As List(Of JToken) = d1.Value.Children().ToList
                Dim e1 As JProperty = e(0)
                'Console.WriteLine(e1.Name)
                'Console.WriteLine(e1.Value)
                'Console.WriteLine(e1.Value("attributeName"))
                If e1.Name = "attribute" Then
                    _outputList.Add(e1.Value("attributeName"))
                Else
                    _outputList.Add(d1.Value)
                End If
            ElseIf c1_ValueList(i).Type = 2 Then
                _outputList.Add(c1_ValueList(i))
            End If

        Next
        'Console.WriteLine(_outputList)
        Return _outputList

    End Function

    Public Function UIDrvt_EleOprt_FreeSeq_Attr(ByVal inputJToken As JToken) As JToken

        Dim c As List(Of JToken) = inputJToken.Children().ToList
        Dim c1 As JProperty = c(0)
        Dim c1_ValueList As List(Of JToken) = c1.Value.ToList

        Dim _outputList As New JArray
        For Each childToken As JObject In c1_ValueList
            Dim d As List(Of JToken) = childToken.Children().ToList
            Dim d1 As JProperty = d(0)
            'Console.WriteLine(d1.Value)
            Dim e As List(Of JToken) = d1.Value.Children().ToList
            Dim e1 As JProperty = e(0)
            'Console.WriteLine(e1.Value)
            'Console.WriteLine(e1.Value("attributeName"))

            If e1.Name = "attribute" Then
                _outputList.Add(e1.Value("attributeName"))
            Else
                _outputList.Add(d1.Value)
            End If

        Next
        'Console.WriteLine(_outputList)
        Return _outputList

    End Function

    Public Function UIDrvt_EleOprt_Conditional_Attr(ByVal inputJToken As JToken) As JToken
        Dim c As List(Of JToken) = inputJToken.Children().ToList
        Dim c1 As JProperty = c(0)
        Dim c1_ValueList As List(Of JToken) = c1.Value.ToList

        Dim outputJArray As New JArray

        For h = 0 To c1_ValueList.Count - 1
            Dim a2_ As List(Of JToken) = c1_ValueList(h).Children.ToList
            Dim a3 As JProperty = a2_(0)
            Dim a4 As List(Of JToken) = a3.Value.ToList

            For k = 0 To a4.Count - 1
                'Console.WriteLine("i = {0}: {1}", k, a4(k))
                Dim a5 As List(Of JToken) = a4(k).Children().ToList
                Dim a6 As JProperty = a5(0)
                'Console.WriteLine(a6.Value)

                Dim a7 As List(Of JToken) = a6.Value.Children().ToList
                Dim a8 As JProperty = a7(0)
                'Console.WriteLine(a8)

                If a8.Name = "attribute" Then
                    outputJArray.Add(a8.Value("attributeName"))
                Else
                    outputJArray.Add(a6.Value)
                End If
            Next
        Next
        'Console.WriteLine(outputJArray)
        Return outputJArray

    End Function







    Public Function BP_Abs_Agg(ByVal inputBP As JObject, ByVal userRole As String) As JToken
        '\\\\ Deal with Strict-order Sequential \\\\
        Dim _1stIdentifiedLevel As Tuple(Of String, List(Of JToken)) = IdentifyTopLevelPattern_1(inputBP)
        Dim _1st_trans_AABP As JToken = EleOperation_StrictOrderSequential(userRole, _1stIdentifiedLevel.Item1, _1stIdentifiedLevel.Item2)

        'Console.WriteLine(_1st_trans_AABP)

        ' \\\How to decompose a JToken of AAed sequential pattern\\\\\\\\\
        Dim a As List(Of JToken) = _1st_trans_AABP.Children().ToList
        Dim a1 As JProperty = a(0)
        Dim a2 As List(Of JToken) = a1.Value.ToList
        'Console.WriteLine(a1.Name)
        'Console.WriteLine(a2)

        For i = 0 To a2.Count - 1
            If a2(i).Type = 1 Then
                'Console.WriteLine(a2(i))
                'Console.WriteLine(a2(i).Type)
                Dim b As List(Of JToken) = a2(i).Children().ToList
                Dim b1 As JProperty = b(0)
                Dim jProperty_Name As String = b1.Name
                If jProperty_Name = "Parallel_A" Then
                    'Console.WriteLine(a2(i))

                    '\\\\ Deal with Parallel_A \\\\
                    Dim patternDetails As Tuple(Of String, List(Of List(Of JToken))) = IdentifyTopLevelPattern_2(a2(i))
                    Dim _transformedMatrix As JToken = EleOperation_ParallelA(userRole, patternDetails.Item1, patternDetails.Item2)

                    ' Console.WriteLine(_transformedMatrix)
                    a2(i) = _transformedMatrix
                End If
            End If
        Next

        Dim _AABP As JObject = New JObject(
         New JProperty(a1.Name, New JArray(
                        From pttnElement In a2
                        Order By a2.IndexOf(pttnElement))))
        Dim AABP As JToken = _AABP

        'AABP is obtained!!!!!!!!!!!!!
        Return AABP

    End Function

    Public Function IdentifyTopLevelPattern_1(ByVal jObject As JObject) As Tuple(Of String, List(Of JToken))
        ' For patterns without branches, like Strict-order Sequetial
        ' !!!!! The input must be a JSON object! (embraced by {})

        'Dim parsedJSON As JObject = JObject.Parse(jsonstring)
        Dim PatternElementList As New List(Of JToken)


        Dim childrenTokens As List(Of JToken) = jObject.Children().ToList ' p.Children(): transform JEnumerable's JTokens to Collection's Tokens  
        ' children(0) is a "JToken", not a "JProperty. 
        ' children(0).Name And children(0).Value Is invalid. 
        ' So children(0) should be transformed to a JProperty to call .Name And .Value
        Dim a As JProperty = childrenTokens(0)

        Dim Pattern_Name As String = a.Name 'Pattern Name is got!!!  a.Value is a JToken

        Dim childTokens As List(Of JToken) = a.Value.ToList

        For Each childToken As JObject In childTokens
            Dim a1 As List(Of JToken) = childToken.Children().ToList
            Dim a2 As JProperty = a1(0)
            PatternElementList.Add(a2.Value)
        Next

        ' Till now, pattern name and list of pattern elements are all obtained!!!

        Dim CtrlFlwPttn As New Tuple(Of String, List(Of JToken))(Pattern_Name, PatternElementList)

        Return CtrlFlwPttn

    End Function

    Public Function IdentifyTopLevelPattern_2(ByVal jObject As JObject) As Tuple(Of String, List(Of List(Of JToken)))
        ' For patterns with branches, like Parallel-A
        ' !!!!! The input must be a JSON object! (embraced by {})

        'Dim parsedJSON As JObject = jObject.Parse(jsonstring)

        Dim PatternElementMatrix As New List(Of List(Of JToken))
        PatternElementMatrix.Add(New List(Of JToken))
        PatternElementMatrix.Add(New List(Of JToken))

        Dim childrenTokens As List(Of JToken) = jObject.Children().ToList
        Dim a As JProperty = childrenTokens(0)
        Dim Pattern_Name As String = a.Name 'Pattern Name is got!!!  a.Value is a JToken

        Dim branchTokens As List(Of JToken) = a.Value.ToList ' Each branch is a JToken

        Dim i = 0
        For Each branchToken As JObject In branchTokens

            Dim branch As List(Of JToken) = branchToken.Children().ToList
            Dim branch_Prop As JProperty = branch(0)
            ' branch_Prop.Name is got!!!
            Dim branchEleTokens As List(Of JToken) = branch_Prop.Value.ToList

            For Each branchEleToken As JObject In branchEleTokens

                Dim branchEle As List(Of JToken) = branchEleToken.Children().ToList
                Dim branchEle_Prop As JProperty = branchEle(0)
                ' branchEle_Prop.Name    branchEle_Prop.Value are got!!!

                PatternElementMatrix(i).Add(branchEle_Prop.Value)
            Next
            i = i + 1
        Next

        ' Till now, pattern name and matrix of pattern elements are all obtained!!!
        Dim CtrlFlwPttn As New Tuple(Of String, List(Of List(Of JToken)))(Pattern_Name, PatternElementMatrix)

        Return CtrlFlwPttn

    End Function

    Public Function EleOperation_StrictOrderSequential(ByVal targetUserRole As String, ByVal patternName As String, ByVal patternEleList As List(Of JToken)) As JToken
        ' We want the AABP for this target user role!
        Dim transformedList As New List(Of JToken)

        Dim _Abs_Node As JObject = New JObject(New JProperty("Abs_Node", "Abs_Node"))
        Dim Abs_Node As JToken = _Abs_Node
        'Console.WriteLine(Abs_Node)

        Dim i As Integer = 0
        Do Until i = patternEleList.Count

            Dim trans_patternElement As List(Of JToken) = patternEleList(i).Children().ToList
            Dim real_patternElement As JProperty = trans_patternElement(0)
            ' real_patternElement.Name is Task or ctrlFlwPttn name.
            ' real_patternElement.value can be task or ctrlFlwPttn. But cannot be an attribute.

            If real_patternElement.Name = "task" Then
                Dim valueObject As JObject = real_patternElement.Value

                If (real_patternElement.Name = "task") And (Not (valueObject("userRole").ToString.Equals(targetUserRole))) Then
                    Dim j = i + 1
                    Do Until j = patternEleList.Count

                        Dim _trans_patternElement As List(Of JToken) = patternEleList(j).Children().ToList
                        Dim _real_patternElement As JProperty = _trans_patternElement(0)

                        If _real_patternElement.Name = "task" Then
                            Dim _valueObject As JObject = _real_patternElement.Value
                            If (_real_patternElement.Name = "task") And (Not (_valueObject("userRole").ToString.Equals(targetUserRole))) Then
                                j = j + 1
                            Else
                                Exit Do
                            End If
                        Else
                            Exit Do
                        End If
                    Loop

                    'transformedList.Add("Abs_Node")
                    transformedList.Add(Abs_Node)

                    i = j
                Else
                    transformedList.Add(patternEleList(i))
                    i = i + 1
                End If
            Else
                transformedList.Add(patternEleList(i))
                i = i + 1
            End If
        Loop

        ' //////Convert the AABP Strict-order Sequential pattern as a JToken//////
        Dim _finalPattern As JObject = New JObject(
         New JProperty(patternName, New JArray(
                        From patternElement In transformedList
                        Order By transformedList.IndexOf(patternElement))))
        Dim PatternJToken As JToken = _finalPattern
        ' //////Convert the AABP Strict-order Sequential pattern as a JToken//////


        ' Return transformedList
        Return PatternJToken
    End Function

    Public Function EleOperation_ParallelA(ByVal targetUserRole As String, ByVal patternName As String, ByVal patternEleMatrix As List(Of List(Of JToken))) As JToken

        Dim transformedMatrix As New List(Of List(Of JToken))
        transformedMatrix.Add(New List(Of JToken))
        transformedMatrix.Add(New List(Of JToken))

        Dim _Abs_Node As JObject = New JObject(New JProperty("Abs_Node", "Abs_Node"))
        Dim Abs_Node As JToken = _Abs_Node

        For i = 0 To patternEleMatrix.Count - 1
            For j = 0 To patternEleMatrix(i).Count - 1
                Dim trans_patternElement As List(Of JToken) = patternEleMatrix.Item(i).Item(j).Children().ToList
                Dim real_patternElement As JProperty = trans_patternElement(0)
                Dim valueObject As JObject = real_patternElement.Value

                If valueObject("userRole").ToString.Equals(targetUserRole) Then
                    transformedMatrix.Item(i).Add(patternEleMatrix.Item(i).Item(j))
                Else
                    transformedMatrix.Item(i).Add(Abs_Node)
                End If
            Next
        Next

        ' Return transformedMatrix



        '//////////////////////////////////////////////////////////////////////////////////

        Dim Branch_1JObject As JToken = Nothing
        Dim Branch_2JObject As JToken = Nothing

        For i = 0 To transformedMatrix.Count - 1
            If i = 0 Then
                Dim _Branch_1JObject As JObject = New JObject(
                        New JProperty("Branch_1", New JArray(
                        From patternElement In transformedMatrix.Item(0)
                        Order By transformedMatrix.Item(0).IndexOf(patternElement))))
                Branch_1JObject = _Branch_1JObject
            ElseIf i = 1 Then
                Dim _Branch_2JObject As JObject = New JObject(
                        New JProperty("Branch_2", New JArray(
                        From patternElement In transformedMatrix.Item(1)
                        Order By transformedMatrix.Item(1).IndexOf(patternElement))))
                Branch_2JObject = _Branch_2JObject
            End If
        Next i

        Dim BranchList As New List(Of JToken)
        BranchList.Add(Branch_1JObject)
        BranchList.Add(Branch_2JObject)

        Dim _finalPattern As JObject = New JObject(
         New JProperty(patternName, New JArray(
                        From patternElement In BranchList
                        Order By BranchList.IndexOf(patternElement))))
        Dim PatternJToken As JToken = _finalPattern

        Return PatternJToken

    End Function


End Module
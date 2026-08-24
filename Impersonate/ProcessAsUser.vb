Imports System.Runtime.InteropServices

<StructLayout(LayoutKind.Sequential)> _
  Public Structure PROCESS_INFORMATION
    Public hProcess As IntPtr
    Public hThread As IntPtr
    Public dwProcessId As System.UInt32
    Public dwThreadId As System.UInt32
End Structure

<StructLayout(LayoutKind.Sequential)> _
Public Structure SECURITY_ATTRIBUTES
    Public nLength As System.UInt32
    Public lpSecurityDescriptor As IntPtr
    Public bInheritHandle As Boolean
End Structure

<StructLayout(LayoutKind.Sequential)> _
Public Structure STARTUPINFO
    Public cb As System.UInt32
    Public lpReserved As String
    Public lpDesktop As String
    Public lpTitle As String
    Public dwX As System.UInt32
    Public dwY As System.UInt32
    Public dwXSize As System.UInt32
    Public dwYSize As System.UInt32
    Public dwXCountChars As System.UInt32
    Public dwYCountChars As System.UInt32
    Public dwFillAttribute As System.UInt32
    Public dwFlags As System.UInt32
    Public wShowWindow As Short
    Public cbReserved2 As Short
    Public lpReserved2 As IntPtr
    Public hStdInput As IntPtr
    Public hStdOutput As IntPtr
    Public hStdError As IntPtr
End Structure

Friend Enum SECURITY_IMPERSONATION_LEVEL
    SecurityAnonymous = 0
    SecurityIdentification = 1
    SecurityImpersonation = 2
    SecurityDelegation = 3
End Enum

Friend Enum TOKEN_TYPE
    TokenPrimary = 1
    TokenImpersonation = 2
End Enum

Public Class ProcessAsUser

    Private Declare Auto Function CreateProcessAsUser Lib "advapi32" ( _
        ByVal hToken As IntPtr, _
        ByVal strApplicationName As String, _
        ByVal strCommandLine As String, _
        ByRef lpProcessAttributes As SECURITY_ATTRIBUTES, _
        ByRef lpThreadAttributes As SECURITY_ATTRIBUTES, _
        ByVal bInheritHandles As Boolean, _
        ByVal dwCreationFlags As Integer, _
        ByVal lpEnvironment As IntPtr, _
        ByVal lpCurrentDriectory As String, _
        ByRef lpStartupInfo As STARTUPINFO, _
        ByRef lpProcessInformation As PROCESS_INFORMATION) As Boolean

    Declare Function DuplicateTokenEx Lib "advapi32.dll" (ByVal hExistingToken As IntPtr, ByVal dwDesiredAccess As System.UInt32, ByRef lpThreadAttributes As SECURITY_ATTRIBUTES, ByVal ImpersonationLevel As Int32, ByVal dwTokenType As Int32, ByRef phNewToken As IntPtr) As Boolean
    Declare Function OpenProcessToken Lib "advapi32.dll" (ByVal ProcessHandle As IntPtr, ByVal DesiredAccess As Integer, ByRef TokenHandle As IntPtr) As Boolean
    Declare Function CreateEnvironmentBlock Lib "userenv.dll" (ByRef lpEnvironment As IntPtr, ByVal hToken As IntPtr, ByVal bInherit As Boolean) As Boolean
    Declare Function DestroyEnvironmentBlock Lib "userenv.dll" (ByVal lpEnvironment As IntPtr) As Boolean

    Private Const SW_SHOW As Short = 5
    Private Const SW_SHOWMAXIMIZED As Short = 7
    Private Const TOKEN_QUERY = 8
    Private Const TOKEN_DUPLICATE = 2
    Private Const TOKEN_ASSIGN_PRIMARY = 1
    Private Const GENERIC_ALL_ACCESS = 268435456
    Private Const STARTF_USESHOWWINDOW = 1
    Private Const STARTF_FORCEONFEEDBACK = 64
    Private Const CREATE_UNICODE_ENVIRONMENT = 1024

    Private Shared Function LaunchProcessAsUser(ByVal cmdLine As String, ByVal token As IntPtr, ByVal envBlock As IntPtr) As Boolean
        Dim result As Boolean = False
        Dim pi As PROCESS_INFORMATION = New PROCESS_INFORMATION
        Dim saProcess As SECURITY_ATTRIBUTES = New SECURITY_ATTRIBUTES
        Dim saThread As SECURITY_ATTRIBUTES = New SECURITY_ATTRIBUTES
        saProcess.nLength = Convert.ToUInt32(Marshal.SizeOf(saProcess))
        saThread.nLength = Convert.ToUInt32(Marshal.SizeOf(saThread))
        Dim si As STARTUPINFO = New STARTUPINFO
        si.cb = Convert.ToUInt32(Marshal.SizeOf(si))
        si.lpDesktop = "WinSta0\Default"
        si.dwFlags = Convert.ToUInt32(STARTF_USESHOWWINDOW Or STARTF_FORCEONFEEDBACK)
        si.wShowWindow = SW_SHOW
        result = CreateProcessAsUser(token, Nothing, cmdLine, saProcess, saThread, True, CREATE_UNICODE_ENVIRONMENT, envBlock, Nothing, si, pi)
        If result = False Then
            Dim Myerror As Integer = Marshal.GetLastWin32Error
            Dim message As String = String.Format("CreateProcessAsUser Error: {0}", Myerror)
            Debug.WriteLine(message)
        End If

        Return result
    End Function

    Private Shared Function GetPrimaryToken(ByVal processId As Integer) As IntPtr
        Dim token As IntPtr = IntPtr.Zero
        Dim primaryToken As IntPtr = IntPtr.Zero
        Dim retVal As Boolean = False
        Dim p As Process = Nothing
        Try
            p = Process.GetProcessById(processId)
        Catch generatedExceptionVariable0 As ArgumentException
            Dim details As String = String.Format("ProcessID {0} Not Available", processId)
            Debug.WriteLine(details)
            Return primaryToken
        End Try
        retVal = OpenProcessToken(p.Handle, TOKEN_DUPLICATE, token)
        If retVal = True Then
            Dim sa As SECURITY_ATTRIBUTES = New SECURITY_ATTRIBUTES
            sa.nLength = Convert.ToUInt32(Marshal.SizeOf(sa))
            retVal = DuplicateTokenEx(token, Convert.ToUInt32(TOKEN_ASSIGN_PRIMARY Or TOKEN_DUPLICATE Or TOKEN_QUERY), sa, CType(SECURITY_IMPERSONATION_LEVEL.SecurityIdentification, Integer), CType(TOKEN_TYPE.TokenPrimary, Integer), primaryToken)
            If retVal = False Then
                Dim message As String = String.Format("DuplicateTokenEx Error: {0}", Marshal.GetLastWin32Error)
                Debug.WriteLine(message)
            End If
        Else
            Dim message As String = String.Format("OpenProcessToken Error: {0}", Marshal.GetLastWin32Error)
            Debug.WriteLine(message)
        End If
        Return primaryToken
    End Function

    Private Shared Function GetEnvironmentBlock(ByVal token As IntPtr) As IntPtr
        Dim envBlock As IntPtr = IntPtr.Zero
        Dim retVal As Boolean = CreateEnvironmentBlock(envBlock, token, False)
        If retVal = False Then
            Dim message As String = String.Format("CreateEnvironmentBlock Error: {0}", Marshal.GetLastWin32Error)
            Debug.WriteLine(message)
        End If
        Return envBlock
    End Function

    Public Shared Function Launch(ByVal appCmdLine As String) As Boolean
        Dim ret As Boolean = False
        Dim ps As Process() = Process.GetProcessesByName("explorer")
        Dim processId As Integer = -1
        If ps.Length > 0 Then
            processId = ps(0).Id
        End If
        If processId > 1 Then
            Dim token As IntPtr = GetPrimaryToken(processId)
            If Not (token.Equals(IntPtr.Zero)) Then
                Dim envBlock As IntPtr = GetEnvironmentBlock(token)
                ret = LaunchProcessAsUser(appCmdLine, token, envBlock)
                If Not (envBlock.Equals(IntPtr.Zero)) Then
                    DestroyEnvironmentBlock(envBlock)
                End If
            End If
        End If
        Return ret
    End Function

End Class

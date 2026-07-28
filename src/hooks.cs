using ArchipelagoFFX.Client;
using Fahrenheit;
using Fahrenheit.Atel;
using Fahrenheit.FFX;
using Fahrenheit.FFX.Battle;
using Fahrenheit.FFX.Ids;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using static ArchipelagoFFX.ArchipelagoData;
using static Fahrenheit.FFX.Globals;
using Color = Archipelago.MultiClient.Net.Models.Color;
using FhGCall = Fahrenheit.FhCall;
using FhXCall = Fahrenheit.FFX.FhCall;

namespace ArchipelagoFFX;

public unsafe partial class ArchipelagoFFXModule {

    private FhMethodHandle<FhGCall.d_CT_RetInt> h_Map_800F
        => new(new FhMethodLocation("FFX.exe", 0x51B1A0));
    private FhMethodHandle<FhGCall.d_CT_RetInt> h_Map_show2DLayerResultInt
        => new(new FhMethodLocation("FFX.exe", 0x51B1A0));
    private FhMethodHandle<FhGCall.d_CT_RetInt> h_Map_hide2DLayerResultInt
        => new(new FhMethodLocation("FFX.exe", 0x51B1E0));
    private FhMethodHandle<FhGCall.d_CT_RetInt> h_Common_obtainBrotherhoodRetInt
        => new(new FhMethodLocation("FFX.exe", 0x459A40));
    private FhMethodHandle<FhGCall.d_CT_Init> h_Common_obtainTreasureInit
        => new(new FhMethodLocation("FFX.exe", 0x45A740));
    private FhMethodHandle<FhGCall.d_CT_RetInt> h_Common_removePartyMemberLongTerm
        => new(new FhMethodLocation("FFX.exe", 0x45AAF0));
    private FhMethodHandle<FhGCall.d_CT_RetInt> h_Common_setPrimerCollected
        => new(new FhMethodLocation("FFX.exe", 0x45AB30));
    private FhMethodHandle<FhGCall.d_CT_RetInt> h_Common_pushParty
        => new(new FhMethodLocation("FFX.exe", 0x45B350));
    private FhMethodHandle<FhGCall.d_CT_RetInt> h_Common_popParty
        => new(new FhMethodLocation("FFX.exe", 0x45B3C0));
    private FhMethodHandle<FhGCall.d_CT_RetInt> h_Common_addPartyMember
        => new(new FhMethodLocation("FFX.exe", 0x45B5A0));
    private FhMethodHandle<FhGCall.d_CT_RetInt> h_Common_removePartyMember
        => new(new FhMethodLocation("FFX.exe", 0x45B6C0));
    private FhMethodHandle<FhGCall.d_CT_RetInt> h_Common_putPartyMemberInSlot
        => new(new FhMethodLocation("FFX.exe", 0x45BC90));
    private FhMethodHandle<FhGCall.d_CT_RetInt> h_Common_0043
        => new(new FhMethodLocation("FFX.exe", 0x45C810));
    private FhMethodHandle<FhGCall.d_CT_RetInt> h_Common_linkFieldToBattleActor
        => new(new FhMethodLocation("FFX.exe", 0x45CA00));
    private FhMethodHandle<FhGCall.d_CT_RetInt> h_Common_playFieldVoiceLineInit
        => new(new FhMethodLocation("FFX.exe", 0x45CB70));
    private FhMethodHandle<FhGCall.d_CT_Exec> h_Common_playFieldVoiceLineExec
        => new(new FhMethodLocation("FFX.exe", 0x45CD30));
    private FhMethodHandle<FhGCall.d_CT_RetInt> h_Common_loadModel
        => new(new FhMethodLocation("FFX.exe", 0x45CE70));
    private FhMethodHandle<FhGCall.d_CT_RetInt> h_Common_playFieldVoiceLineResultInt
        => new(new FhMethodLocation("FFX.exe", 0x45D150));
    private FhMethodHandle<FhGCall.d_CT_RetInt> h_Common_00D6Init
        => new(new FhMethodLocation("FFX.exe", 0x45D520));
    private FhMethodHandle<FhGCall.d_CT_Exec> h_Common_00D6Exec
        => new(new FhMethodLocation("FFX.exe", 0x45D820));
    private FhMethodHandle<FhGCall.d_CT_RetInt> h_Common_00D6ResultInt
        => new(new FhMethodLocation("FFX.exe", 0x45DCF0));
    private FhMethodHandle<FhGCall.d_CT_Init> h_Common_01D1Init
        => new(new FhMethodLocation("FFX.exe", 0x45FB60));
    private FhMethodHandle<FhGCall.d_CT_Exec> h_Common_01D1Exec
        => new(new FhMethodLocation("FFX.exe", 0x45FDB0));
    private FhMethodHandle<FhGCall.d_CT_RetInt> h_Common_setWeaponInvisible
        => new(new FhMethodLocation("FFX.exe", 0x456770));
    private FhMethodHandle<FhGCall.d_CT_Init> h_Common_obtainTreasureSilentlyInit
        => new(new FhMethodLocation("FFX.exe", 0x4579E0));
    private FhMethodHandle<FhGCall.d_CT_RetInt> h_CT_RetInt_0065
        => new(new FhMethodLocation("FFX.exe", 0x457F60));
    private FhMethodHandle<FhGCall.d_CT_RetInt> h_Common_transitionToMap
        => new(new FhMethodLocation("FFX.exe", 0x4580C0));
    private FhMethodHandle<FhGCall.d_CT_RetInt> h_Common_warpToMap
        => new(new FhMethodLocation("FFX.exe", 0x458370));
    private FhMethodHandle<FhGCall.d_CT_RetInt> h_CT_RetInt_006A
        => new(new FhMethodLocation("FFX.exe", 0x4589F0));
    private FhMethodHandle<FhGCall.d_CT_RetInt> h_CT_RetInt_01B6
        => new(new FhMethodLocation("FFX.exe", 0x4594D0));
    private FhMethodHandle<FhGCall.d_CT_RetInt> h_Common_upgradeBrotherhoodRetInt
        => new(new FhMethodLocation("FFX.exe", 0x4596A0));
    private FhMethodHandle<FhGCall.d_CT_Init> h_SgEvent_showModularMenuInit
        => new(new FhMethodLocation("FFX.exe", 0x678210));

    public static int* takara_pointer => FhUtil.ptr_at<int>(0xD35FEC);
    public static int* buki_get_pointer => FhUtil.ptr_at<int>(0xD35FF4);

    public static char* get_event_name(uint event_id) => FhXCall.AtelGetEventName.fnptr!(event_id);
    public static int atel_stack_pop(int* arg1, AtelStack* atelStack) => FhXCall.AtelPopStackInteger.fnptr!(arg1, atelStack);

    public static void AtelSetUpCallFunc(int id, nint nameSpacePtr) => FhXCall.AtelSetUpCallFunc.fnptr!(id, nameSpacePtr);

    public void init_hooks() {
        foreach (byte[] script in customScripts) {
            customScriptHandles.Add(GCHandle.Alloc(script, GCHandleType.Pinned));
        }
    }

    public static int ignore_this = 11;
    public int h_Map_800F_reimpl(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        int param_1 = atelStack->values.as_int()[0];
        if (param_1 == ignore_this) {
            atelStack->pop_int();
            return 1;
        }
        return h_Map_800F.chain_from(h_Map_800F_reimpl).fnptr!(work, storage, atelStack);
    }

    //public bool h_openFile(nint _this, nint filename, bool readOnly, nint unknown_1, nint unknown_2, nint unknown_3) {
    //    string x = Marshal.PtrToStringAnsi(filename)!;
    //    _logger.Debug($"{_this}, {x}, {readOnly}, {unknown_1}, {unknown_2}, {unknown_3}");
    //    var result = FhXCall.Phyre_PSerialization_PStreamFileWin32_Open.chain_from(h_openFile).fnptr!(_this, filename, readOnly, unknown_1, unknown_2, unknown_3);
    //    _logger.Debug($"{result}, {*(int*)_this}, {*(int*)((int)_this + 4)}");
    //    return result;
    //}

    public uint h_FUN_0070aec0(nint _this, uint voice_id, uint param_2) {
        return FhXCall.FUN_0070aec0.chain_from(h_FUN_0070aec0).fnptr!(_this, voice_id, param_2);
    }

    private FhGCall.d_CT_RetInt[] custom_ct_retints;
    private int   turnAroundScriptLength;
    private byte* turnAroundScript;
    private void init_custom_atel() {
        custom_ct_retints = [
            CT_RetInt_PushInt,
            CT_RetInt_PushFloat,
            CT_RetInt_IsCharacterUnlocked,
            CT_RetInt_HasCelestialWeapon,
            CT_RetInt_IsOtherLocationChecked,
            CT_RetInt_IsTreasureLocationChecked,
            CT_RetInt_CollectedPrimers,
            CT_RetInt_SendOtherLocation,
            CT_RetInt_SendPartyMemberLocation,
            CT_RetInt_SendRecruitLocation,
            CT_RetInt_BlockWarp,
            CT_RetInt_BlockKilikaBoatChoice,
            CT_RetInt_RestoreInteraction,
            CT_RetInt_SetAirshipDestinations,
            CT_RetInt_ShowAirshipDestinations,
            CT_RetInt_HideAirshipDestinations,
            CT_RetInt_ShowCurrentAirshipLocation,
            CT_RetInt_HideCurrentAirshipLocation,
            CT_RetInt_TransitionToRegion,
            CT_RetInt_Jump,
            CT_RetInt_JumpIfFalse,
            CT_RetInt_JumpIfTrue,
            CT_RetInt_Offset,
            CT_RetInt_OffsetIfFalse,
            CT_RetInt_OffsetIfTrue,
            CT_RetInt_UpdateRegionState,
            CT_RetInt_LockPartyMember,
            CT_RetInt_LockAllAeons,
            CT_RetInt_IsGoalUnlocked,
            CT_RetInt_ReplaceEntryPoint,
            CT_RetInt_RestoreEntryPoint,
            CT_RetInt_LightningDodging,
            CT_RetInt_JechtSphere,
            CT_RetInt_KickedBlitzballAway,
            CT_RetInt_CheckUnlockedAeons,
        ];

        customNameSpace = new AtelCallTarget[custom_ct_retints.Length];

        for (int i = 0; i < custom_ct_retints.Length; i++) {
            customNameSpace[i].ret_int_func = Marshal.GetFunctionPointerForDelegate(custom_ct_retints[i]);
        }

        customNameSpaceHandle = GCHandle.Alloc(customNameSpace, GCHandleType.Pinned);

        turnAroundScriptLength = atelTurnAround(Span<byte>.Empty, 0, 0, 0, 0, 0, 0);
        turnAroundScript       = (byte*)NativeMemory.AllocZeroed((uint)turnAroundScriptLength);

    }

    private bool hook() {
        FhApi.Events.Common.GameLoop.PreUpdate.subscribe(handle_input);

        return h_Common_obtainTreasureInit.hook(this, Common_obtainTreasureInit)
            && h_Common_obtainTreasureSilentlyInit.hook(this, Common_obtainTreasureSilentlyInit)
            && h_Common_obtainBrotherhoodRetInt.hook(this, Common_obtainBrotherhoodRetInt)
            && h_Common_setPrimerCollected.hook(this, Common_setPrimerCollected)
            && h_Common_transitionToMap.hook(this, Common_transitionToMap)
            && h_Common_warpToMap.hook(this, Common_warpToMap)
            && h_SgEvent_showModularMenuInit.hook(this, SgEvent_showModularMenuInit)
            && h_Common_addPartyMember.hook(this, Common_addPartyMember)
            && h_Common_removePartyMember.hook(this, Common_removePartyMember)
            && h_Common_removePartyMemberLongTerm.hook(this, Common_removePartyMemberLongTerm)
            && h_Common_setWeaponInvisible.hook(this, Common_setWeaponInvisible)
            && h_Common_putPartyMemberInSlot.hook(this, Common_putPartyMemberInSlot)
            && h_Common_pushParty.hook(this, Common_pushParty)
            && h_Common_popParty.hook(this, Common_popParty)
            && FhXCall.TkSetLegendAbility.hook(this, TkSetLegendAbility)
            && FhXCall.AtelEventSetUp.hook(this, AtelEventSetUp)
            && FhXCall.MsBattleExe.hook(this, MsBattleExe)
            && FhXCall.FUN_00791820.hook(this, FUN_00791820)
            && FhXCall.MsApUp.hook(this, MsApUp)
            && FhXCall.MsBtlReadSetScene.hook(this, MsBtlReadSetScene)
            && FhXCall.eiAbmParaGet.hook(this, eiAbmParaGet)
            && FhXCall.MsSetSaveParam.hook(this, MsSetSaveParam)
            && FhXCall.MsSetRamChrParam.hook(this, MsSetRamChrParam)
            && FhXCall.FUN_0086bec0.hook(this, FUN_0086bec0)
            && FhXCall.FUN_0086bea0.hook(this, FUN_0086bea0) // Custom strings
            && FhXCall.graphicInitFMVPlayer.hook(this, graphicInitFMVPlayer)
            && FhXCall.FmodVoice_dataChange.hook(this, FmodVoice_dataChange)
            && FhXCall.AtelInitTotal.hook(this, AtelInitTotal)
            && FhXCall.LocalizationManager_Initialize.hook(this, LocalizationManager_Initialize)
            && FhXCall.TkMenuAppearMainCmdWindow.hook(this, TkMenuAppearMainCmdWindow)
            && FhXCall.FUN_008c2370.hook(this, PrepareMenuList)
            && FhXCall.UpdateGearCustomizationMenuState.hook(this, UpdateGearCustomizationMenuState)
            && FhXCall.DrawGearCustomizationMenu.hook(this, DrawGearCustomizationMenu)
            && FhXCall.TkMenuCtrlSummon.hook(this, TkMenuCtrlSummon)
            && FhXCall.FUN_008cdb70.hook(this, DrawAeonCustomizationMenu)
            && FhXCall.FUN_008d5720.hook(this, FUN_008d5720)
            && FhXCall.TODrawWindow.hook(this, render_game);
        //  && _FUN_00656c90.hook() && _FUN_0065ee30.hook();
        //  && _openFile.hook() && _FUN_0070aec0.hook();
        //  && _MsCheckLeftWindow.hook() && _MsCheckUseCommand.hook() && _TOBtlDrawStatusLimitGauge.hook();
        //  && Map_800F //MsBtlGetPos
        //  && FUN_00a48910

    }

    private void set(byte* code_ptr, uint offset, AtelInst opcode) {
        byte* ptr = code_ptr + offset;
        foreach (byte b in opcode.to_bytes()) {
            *ptr = b;
            ptr++;
        }
    }
    private void set(byte* code_ptr, uint[] offsets, AtelInst opcode) {
        foreach (uint offset in offsets) {
            set(code_ptr, offset, opcode);
        }
    }

    private  void set(byte* code_ptr, uint offset, AtelInst[] opcodes) {
        byte* ptr = code_ptr + offset;
        foreach (AtelInst op in opcodes) {
            foreach (byte b in op.to_bytes()) {
                *ptr = b;
                ptr++;
            }
        }
    }
    private  void set(byte* code_ptr, uint[] offsets, AtelInst[] opcodes) {
        foreach (uint offset in offsets) {
            set(code_ptr, offset, opcodes);
        }
    }

    private void move(byte* code_ptr, uint sourceOffset, int moveBy, int size) {
        Span<Byte> source      = new(code_ptr + sourceOffset         , size);
        Span<Byte> destination = new(code_ptr + sourceOffset + moveBy, size);

        Span<Byte> destinationSource;
        Span<Byte> destinationDestination;
        int destinationSize = Math.Abs(moveBy);
        if (moveBy < 0) {
            destinationSource      = new(code_ptr + sourceOffset + moveBy       , destinationSize);
            destinationDestination = new(code_ptr + sourceOffset + moveBy + size, destinationSize);
        } else {
            destinationSource      = new(code_ptr + sourceOffset + size, destinationSize);
            destinationDestination = new(code_ptr + sourceOffset       , destinationSize);
        }

        byte[] temp = new byte[destinationSize];
        destinationSource.CopyTo(temp);

        source.CopyTo(destination);
        temp.CopyTo(destinationDestination);
    }

    private byte* FUN_0086bec0(int param_1) {
        byte* result;
        if ((param_1 & 0x8000) != 0) {
            int custom_index = param_1 & 0x7FFF;
            result = customStrings[custom_index].encoded;
            //result = (byte*)customStringHandles[custom_index].AddrOfPinnedObject();
            //FhEncoding.compute_decode_buffer_size(result);
            //string decoded = FhEncoding.Us.to_string(result);
            _logger.Debug(customStrings[custom_index].decoded);
        } else {
            // May crash if called with invalid index
            result = FhXCall.FUN_0086bec0.chain_from(FUN_0086bec0).fnptr!(param_1);
        }

        return result;
    }

    private short FUN_0086bea0(int param_1) {
        short result;
        if ((param_1 & 0x8000) != 0) {
            int custom_index = param_1 & 0x7FFF;
            result = customStrings[custom_index].metadata;
        }
        else {
            result = FhXCall.FUN_0086bea0.chain_from(FUN_0086bea0).fnptr!(param_1);
        }

        return result;
    }

    private static readonly byte NEWLINE = 0x03;
    private static readonly byte TIME    = 0x09;
    private static readonly byte CHOICE  = 0x10;

    //private static readonly List<GCHandle> customStringHandles = [];
    //public static readonly byte[][] customStrings = {
    //    //new byte[][] {
    //    //    [TIME, 0x30],
    //    //    FhEncoding.Us.to_bytes("Ready to fight Sin?"),
    //    //    [NEWLINE, CHOICE, 0x30],
    //    //    FhEncoding.Us.to_bytes("Yes"),
    //    //    [NEWLINE, CHOICE, 0x31],
    //    //    FhEncoding.Us.to_bytes("No"),
    //    //}.SelectMany(x => x).ToArray(),
    //    //FhEncoding.Us.to_bytes("{TIME:0}Ready to fight {COLOR:BLUE}Sin{COLOR:WHITE}?\n{CHOICE:0}Yes\n{CHOICE:1}No"),
    //};

    public struct ManagedCustomString {
        public byte[] encoded;

        public ManagedCustomString(ReadOnlySpan<byte> text, FhEncodingFlags encodingFlags = default) {

            int encodedLength = FhEncoding.compute_encode_buffer_size(text, flags: encodingFlags);
            this.encoded = new byte[encodedLength + 1];
            int actual_size = FhEncoding.encode(text, encoded, flags: encodingFlags);
        }
        public ManagedCustomString(string text, FhEncodingFlags encodingFlags = default) {

            ReadOnlySpan<byte> utf8String = Encoding.UTF8.GetBytes(text);

            int encodedLength = FhEncoding.compute_encode_buffer_size(utf8String, flags: encodingFlags);
            this.encoded = new byte[encodedLength + 1];
            int actual_size = FhEncoding.encode(utf8String, encoded, flags: encodingFlags);
        }
    }
    public struct NativeCustomString {
        public byte* encoded;
        public int encodedLength;
        public string decoded;
        public int choices;
        public int flags;

        public NativeCustomString(ReadOnlySpan<byte> text, int choices = 0, int flags = 0, FhEncodingFlags encodingFlags = default) {
            this.choices = choices;
            this.flags = flags;

            this.decoded = Encoding.UTF8.GetString(text);

            this.encodedLength = FhEncoding.compute_encode_buffer_size(text, flags: encodingFlags);
            this.encoded = (byte*)NativeMemory.AllocZeroed((nuint)encodedLength + 1);
            int actual_size = FhEncoding.encode(text, new Span<byte>(encoded, encodedLength), flags: encodingFlags);
        }

        public NativeCustomString(string text, int choices = 0, int flags = 0, FhEncodingFlags encodingFlags = default) {
            this.choices = choices;
            this.flags = flags;
            this.decoded = text;

            ReadOnlySpan<byte> utf8String = Encoding.UTF8.GetBytes(text);

            this.encodedLength = FhEncoding.compute_encode_buffer_size(utf8String, flags: encodingFlags);
            this.encoded = (byte*)NativeMemory.AllocZeroed((nuint)encodedLength + 1);
            int actual_size = FhEncoding.encode(utf8String, new Span<byte>(encoded, encodedLength), flags: encodingFlags);
        }

        public readonly void Free() {
            NativeMemory.Free(encoded);
        }

        public readonly short metadata => (short)((choices << 8) | flags);
    }

    //private const byte WHITE     = 0x41;
    //private const byte YELLOW    = 0x43;
    //private const byte GREY      = 0x52;
    //private const byte BLUE      = 0x88;
    //private const byte RED       = 0x94;
    //private const byte PINK      = 0x97;
    //private const byte OL_PURPLE = 0xA1;
    //private const byte OL_CYAN   = 0xB1;

    public static readonly NativeCustomString[] customStrings = [
        new NativeCustomString("{TIME:00}Ready to fight {COLOR:88}Sin{COLOR:41}?{LF}{CHOICE:00}Yes{LF}{CHOICE:01}No"u8, 2, 0),
        new NativeCustomString("{TIME:00}Ready to fight {COLOR:88}Jecht{COLOR:41}?{LF}{CHOICE:00}Yes{LF}{CHOICE:01}No"u8, 2, 0),
        new NativeCustomString("{TIME:00}Locked"u8, 0, 0),
        new NativeCustomString("{TIME:00}{CHOICE:00}Save{LF}{CHOICE:01}Board airship{LF}{CHOICE:02}Play blitzball{LF}{CHOICE:03}Cancel"u8, 4, 0),
        new NativeCustomString("{TIME:00}The {MACRO:06:42} are not at full{LF}strength. You need more members{LF}to participate in blitzball."u8, 0, 0),
        new NativeCustomString("{TIME:00}Skip ending?{LF}{CHOICE:00}Yes{LF}{CHOICE:01}No"u8, 2, 0),
        new NativeCustomString("{TIME:00}Other regions must be accessed from the Airship Menu"u8, 0, 0),
    ];
    //public static readonly byte[][] rawCustomStrings = [
    //    "{TIME:0}Ready to fight {COLOR:BLUE}Sin{COLOR:WHITE}?\n{CHOICE:0}Yes\n{CHOICE:1}No"u8.ToArray()
    //];


    // Choices << 8 | Flags
    //private static readonly short[] customStringFlags = {
    //    (02 << 8) | 00,
    //
    //};


    //private static readonly byte[] sinString = new byte[][] {
    //        [TIME, 0x30],
    //        FhEncoding.Us.to_bytes("Ready to fight Sin?"),
    //        [NEWLINE, CHOICE, 0x30],
    //        FhEncoding.Us.to_bytes("Yes"),
    //        [NEWLINE, CHOICE, 0x31],
    //        FhEncoding.Us.to_bytes("No"),
    //    }.SelectMany(x => x).ToArray();

    private static readonly GCHandle APItemString;
    //private static readonly GCHandle APItemString = GCHandle.Alloc(FhEncoding.encode("Archipelago Item"), GCHandleType.Pinned);

    private static AtelInst[] atelDisplayFieldString(ushort boxIndex, ushort stringIndex, ushort x, ushort y, ushort align = 4, ushort textFlags = 0, ushort transparent = 0) {
        return [
            // call Common.positionText [0065h](boxIndex=1 [01h], x=256 [0100h], y=224 [E0h], align=Center [04h]);
            AtelOp.PUSHII   .build(boxIndex),
            AtelOp.PUSHII   .build(x),
            AtelOp.PUSHII   .build(y),
            AtelOp.PUSHII   .build(align),
            AtelOp.CALLPOPA .build(0x0065),
            // call Common.setTextHasTransparentBackdrop [0066h](boxIndex=1 [01h], transparent=false [00h]);
            AtelOp.PUSHII   .build(boxIndex),
            AtelOp.PUSHII   .build(transparent),
            AtelOp.CALLPOPA .build(0x0066),
            // call Common.displayFieldString [0064h](boxIndex=1 [01h], string=customString[2] [00h]);
            AtelOp.PUSHII   .build(boxIndex),
            AtelOp.PUSHII   .build(stringIndex),
            AtelOp.CALLPOPA .build(0x0064),
            // call Common.setTextFlags [009Dh](boxIndex=1 [01h], textFlags=[] [00h]);
            AtelOp.PUSHII   .build(boxIndex),
            AtelOp.PUSHII   .build(textFlags),
            AtelOp.CALLPOPA .build(0x009D),
            // call Common.006A(boxIndex=1 [01h], p2=0 [00h]);
            AtelOp.PUSHII   .build(boxIndex),
            AtelOp.PUSHII   .build(0x0000),
            AtelOp.CALLPOPA .build(0x006A),
        ];
    }

    private static AtelInst[] atelPushInt(uint value) {
        return [
            AtelOp.PUSHII  .build((ushort)(value & 0xffff)),
            AtelOp.PUSHII  .build((ushort)(value >>    16)),
            AtelOp.CALLPOPA.build((ushort)CustomCallTarget.PUSH_INT),
            ];
    }

    private static AtelInst[] atelPushFloat(float value) {
        int as_int = BitConverter.SingleToInt32Bits(value);
        return [
            AtelOp.PUSHII  .build((ushort)(as_int & 0xffff)),
            AtelOp.PUSHII  .build((ushort)(as_int >>    16)),
            AtelOp.CALLPOPA.build((ushort)CustomCallTarget.PUSH_FLOAT),
            ];
    }

    private static AtelInst[] atelPushIntAsFloat(uint value) {
        return [
            AtelOp.PUSHII  .build((ushort)(value & 0xffff)),
            AtelOp.PUSHII  .build((ushort)(value >>    16)),
            AtelOp.CALLPOPA.build((ushort)CustomCallTarget.PUSH_FLOAT),
            ];
    }

    private static AtelInst[] atelNOPArray(uint n) {
        AtelInst[] temp = new AtelInst[n];
        temp.AsSpan().Fill(AtelOp.NOP.build());
        //for (int i = 0; i < temp.Length; i++) {
        //    temp[i] = AtelOp.NOP.build();
        //}
        return temp;
    }

    private readonly List<GCHandle> customScriptHandles = [];
    private readonly byte[][] customScripts = {
        // Cid talk hook
        ((AtelInst[])[ // 0
            // If GameMoment < 2970 or GameMoment >= 3120: Jump to j01 (return)
            AtelOp.PUSHV   .build(0x0000),
            AtelOp.POPY    .build(      ),

            AtelOp.PUSHY   .build(      ),
            AtelOp.PUSHII  .build(2970  ),
            AtelOp.LS      .build(      ),

            AtelOp.PUSHY   .build(      ),
            AtelOp.PUSHII  .build(3210  ),
            AtelOp.GTE     .build(      ),

            AtelOp.LOR     .build(      ),
            AtelOp.POPXCJMP.build(0x0001),


            // call Common.disablePlayerControl? [005Eh]();
            AtelOp.CALLPOPA.build(0x005e),
            // call Common.displayFieldChoice [013Bh](boxIndex=1 [01h], string=customString[0], p3=0 [00h], p4=1 [01h], x=280 [0118h], y=224 [E0h], align=Center [04h]);
            AtelOp.PUSHII  .build(0x0001),
            AtelOp.PUSHII  .build(0x8000),
            AtelOp.PUSHII  .build(0x0000),
            AtelOp.PUSHII  .build(0x0001),
            AtelOp.PUSHII  .build(0x0100),
            AtelOp.PUSHII  .build(0x00e0),
            AtelOp.PUSHII  .build(0x0004),
            AtelOp.CALLPOPA.build(0x013b),
            AtelOp.PUSHA.build(),
            // call Common.waitForText [0084h](boxIndex=1 [01h], p2=1 [01h]);
            AtelOp.PUSHII  .build(0x0001),
            AtelOp.PUSHII  .build(0x0001),
            AtelOp.CALLPOPA.build(0x0084),
            // call Common.enablePlayerControl? [005Dh]();
            AtelOp.CALLPOPA.build(0x005d),
            // call Custom.JumpIfFalse(choice, 1) i.e. if !choice jump to customscripts[1]
            AtelOp.PUSHII  .build(0x0001),
            AtelOp.CALLPOPA.build((ushort)CustomCallTarget.JUMP_IF_FALSE),


            //// Jump to j01 (return)
            AtelOp.JMP     .build(0x0001),
        ]).SelectMany(x => x.to_bytes()).ToArray(),

        // Start Airship Sin fights
        ((AtelInst[])[ // 1
            // Set GameMoment = 3085
            AtelOp.PUSHII  .build(  3085),
            AtelOp.POPV    .build(0x0000),
            // call Common.transitionToMap? [0011h](map=199, entranceIndex=0);
            AtelOp.PUSHII  .build(   199),
            AtelOp.PUSHII  .build(     0),
            AtelOp.CALLPOPA.build(0x0011),
            // Return;
            AtelOp.RET.build(),

        ]).SelectMany(x => x.to_bytes()).ToArray(),

        // Remove defeated Aeon
        ((AtelInst[])[ // 2
            // call Custom.LockPartyMember(privBA98)
            AtelOp.PUSHV   .build(0x000B),
            AtelOp.CALLPOPA.build((ushort)CustomCallTarget.LOCK_PARTY_MEMBER),

            // switch privBA98
            AtelOp.PUSHV   .build(0x000B),
            AtelOp.POPY    .build(),
            // Jump to j04 (6758)
            AtelOp.JMP     .build(0x0004),

        ]).SelectMany(x => x.to_bytes()).ToArray(),

        // Check Goal Requirement (Falling tower)
        ((AtelInst[])[ // 3
            // If !(GameMoment < 3250): Jump to j01 (return)
            AtelOp.PUSHV    .build(0x0000),
            AtelOp.PUSHII   .build(3250  ),
            AtelOp.LS       .build(      ),
            AtelOp.POPXNCJMP.build(0x0001),
            // call Custom.IsGoalUnlocked()
            AtelOp.CALLPOPA .build((ushort)CustomCallTarget.IS_GOAL_UNLOCKED),
            // call Common.JumpIfTrue(isGoalUnlocked, 4) i.e. if isGoalUnlocked jump to customScripts[4]
            AtelOp.PUSHA    .build(),
            AtelOp.PUSHII   .build(0x0004),
            AtelOp.CALLPOPA .build((ushort)CustomCallTarget.JUMP_IF_TRUE),

            // call Common.disablePlayerControl? [005Eh]();
            AtelOp.CALLPOPA.build(0x005e),
            // display customStrings[2]
            .. atelDisplayFieldString(1, 0x8002, 256, 224, 4, 0, 0),

            // walk away
            // call Common.ReplaceEntryPoint(w0, 6, 5) i.e. workers[0].entry_points[6] = customScripts[5]
            AtelOp.PUSHII  .build(0x0000),
            AtelOp.PUSHII  .build(0x0006),
            AtelOp.PUSHII  .build(0x0005),
            AtelOp.CALLPOPA.build((ushort)CustomCallTarget.REPLACE_ENTRY_POINT),

            // runAndAwaitEnd w00e06 (Level 1)
            AtelOp.PUSHII  .build(0x0001),
            AtelOp.PUSHII  .build(0x0000),
            AtelOp.PUSHII  .build(0x0006),
            AtelOp.REQEW   .build(      ),
            AtelOp.POPA    .build(      ),

            // call Common.RestoreEntryPoint(w0, 6) i.e. workers[0].entry_points[6] = original value
            AtelOp.PUSHII  .build(0x0000),
            AtelOp.PUSHII  .build(0x0006),
            AtelOp.CALLPOPA.build((ushort)CustomCallTarget.RESTORE_ENTRY_POINT),

            // call Common.waitForText [0084h](boxIndex=1 [01h], p2=1 [01h]);
            AtelOp.PUSHII  .build(0x0001),
            AtelOp.PUSHII  .build(0x0001),
            AtelOp.CALLPOPA.build(0x0084),
            // call Common.enablePlayerControl? [005Dh]();
            AtelOp.CALLPOPA.build(0x005d),

            // Jump to j01 (return)
            AtelOp.JMP      .build(0x0001),

        ]).SelectMany(x => x.to_bytes()).ToArray(),

        ((AtelInst[])[ // 4
            // call Common.disablePlayerControl? [005Eh]();
            AtelOp.CALLPOPA.build(0x005e),
            // call Common.displayFieldChoice [013Bh](boxIndex=1 [01h], string=customString[1], p3=0 [00h], p4=1 [01h], x=280 [0118h], y=224 [E0h], align=Center [04h]);
            AtelOp.PUSHII    .build(0x0001),
            AtelOp.PUSHII    .build(0x8001),
            AtelOp.PUSHII    .build(0x0000),
            AtelOp.PUSHII    .build(0x0001),
            AtelOp.PUSHII    .build(0x0100),
            AtelOp.PUSHII    .build(0x00e0),
            AtelOp.PUSHII    .build(0x0004),
            AtelOp.CALLPOPA  .build(0x013b),
            AtelOp.PUSHA     .build(),
            // call Common.waitForText [0084h](boxIndex=1 [01h], p2=1 [01h]);
            AtelOp.PUSHII    .build(0x0001),
            AtelOp.PUSHII    .build(0x0001),
            AtelOp.CALLPOPA  .build(0x0084),
            // call Common.enablePlayerControl? [005Dh]();
            AtelOp.CALLPOPA  .build(0x005d),
            // If !choice Jump to j00 (continue)
            AtelOp.POPXNCJMP  .build(0x0000),

            // walk away
            // call Common.RestoreEntryPoint(w0, 6, 5) i.e. workers[0].entry_points[6] = customScripts[5]
            AtelOp.PUSHII  .build(0x0000),
            AtelOp.PUSHII  .build(0x0006),
            AtelOp.PUSHII  .build(0x0005),
            AtelOp.CALLPOPA.build((ushort)CustomCallTarget.REPLACE_ENTRY_POINT),

            // runAndAwaitEnd w00e06 (Level 1)
            AtelOp.PUSHII  .build(0x0001),
            AtelOp.PUSHII  .build(0x0000),
            AtelOp.PUSHII  .build(0x0006),
            AtelOp.REQEW   .build(      ),
            AtelOp.POPA    .build(      ),

            // call Common.RestoreEntryPoint(w0, 6) i.e. workers[0].entry_points[6] = original value
            AtelOp.PUSHII  .build(0x0000),
            AtelOp.PUSHII  .build(0x0006),
            AtelOp.CALLPOPA.build((ushort)CustomCallTarget.RESTORE_ENTRY_POINT),

            // call Common.enablePlayerControl? [005Dh]();
            AtelOp.CALLPOPA.build(0x005d),

            // Jump to j01 (return)
            AtelOp.JMP       .build(0x0001),
        ]).SelectMany(x => x.to_bytes()).ToArray(),

        ((AtelInst[])[ // 5
            //call Common.disablePlayerControl? [005Eh]();
            AtelOp.CALLPOPA.build(0x005E),
            //call Common.stopWorkerMotion [00D1h]();
            AtelOp.CALLPOPA.build(0x00D1),
            //call Common.setMovementSpeed [006Ch](speed=34.0 [42080000h]);
            AtelOp.PUSHF   .build(0x0008),
            AtelOp.CALLPOPA.build(0x006C),
            //call Common.0017(p1=0 [00h]);
            AtelOp.PUSHII  .build(0x0000),
            AtelOp.CALLPOPA.build(0x0017),
            //call Common.setRotationSpeed1 [002Eh](float=0.10471976 [3DD67750h]);
            AtelOp.PUSHF   .build(0x0006),
            AtelOp.CALLPOPA.build(0x002E),
            //call Common.setRotationSpeed2 [002Fh](float=0.17453294 [3E32B8C3h]);
            AtelOp.PUSHF   .build(0x0007),
            AtelOp.CALLPOPA.build(0x002F),
            //call Common.setDestination [0015h](x=-4 [c0800000h], y=5.4481072 [40ae56e5h], z=-350 [c3af0000h]);
            .. atelPushFloat(-4f),
            .. atelPushFloat(5.4481072f),
            .. atelPushFloat(-350f),
            AtelOp.CALLPOPA.build(0x0015),
            //call Common.setRotationTarget1 [0028h](angle=Common.destinationToYaw [001Fh]());
            AtelOp.CALL    .build(0x001F),
            AtelOp.CALLPOPA.build(0x0028),
            //call Common.startRotation [0019h](activeBits=0 [00h], flags=[b0002, b1000, b4000, b8000] [0000D002h], targetWorker=w00 [00h]);
            AtelOp.PUSHII  .build(0x0000),
            AtelOp.PUSHI   .build(0x0000),
            AtelOp.PUSHII  .build(0x0000),
            AtelOp.CALLPOPA.build(0x0019),
            //call Common.startMotion [0018h](activeBits=0 [00h], flags=[b0001, b4000, b8000] [0000C001h], targetWorker=w00 [00h]);
            AtelOp.PUSHII  .build(0x0000),
            AtelOp.PUSHI   .build(0x0001),
            AtelOp.PUSHII  .build(0x0000),
            AtelOp.CALLPOPA.build(0x0018),
            //call Common.waitForMotion [001Ah]();
            AtelOp.CALLPOPA.build(0x001A),
            //call Common.stopWorkerRotation [0078h](worker=Common.getWorkerIndex [0033h](worker=<Self> [FFFFh]));
            AtelOp.PUSHII  .build(0xFFFF),
            AtelOp.CALL    .build(0x0033),
            AtelOp.CALLPOPA.build(0x0078),
            //return;
            AtelOp.RET     .build(      ),

        ]).SelectMany(x => x.to_bytes()).ToArray(),

        // bvyt0900 Save Sphere (Options)
        ((AtelInst[])[ // 6
            // (0 [00h] == case) -> j03 (5170)
            AtelOp.PUSHII  .build(0),
            AtelOp.PUSHY   .build( ),
            AtelOp.EQ      .build( ),
            AtelOp.POPXCJMP.build(3),
            // (1 [00h] == case) -> customscripts[7] (5170)
            AtelOp.PUSHII  .build(1),
            AtelOp.PUSHY   .build( ),
            AtelOp.EQ      .build( ),
            //AtelOp.POPXCJMP.build(3),
            // call Common.JumpIfTrue(value, 7) i.e. if value jump to customscripts[7]
            AtelOp.PUSHII  .build(0x0007),
            AtelOp.CALLPOPA.build((ushort)CustomCallTarget.JUMP_IF_TRUE),
            // (2 [00h] == case) -> j03 (5170)
            AtelOp.PUSHII  .build(2),
            AtelOp.PUSHY   .build( ),
            AtelOp.EQ      .build( ),
            // call Common.JumpIfTrue(value, 8) i.e. if value jump to customscripts[8]
            AtelOp.PUSHII  .build(0x0008),
            AtelOp.CALLPOPA.build((ushort)CustomCallTarget.JUMP_IF_TRUE),
            // Jump to j02 (51A8)
            AtelOp.JMP     .build(2),


        ]).SelectMany(x => x.to_bytes()).ToArray(),

        // bvyt0900 Save Sphere (Board Airship option)
        ((AtelInst[])[ // 7
            // AD3A00 D8DF00           call Common.playSfx [00DFh](sfx=BoardAirship [80000048h]);
            .. atelPushInt(0x80000048),
            AtelOp.CALLPOPA.build(0x00DF),
            // AE0300 AE0000 D80980    call Map.bindGfxToTarget [8009h](gfxIndex= 3[03h], attachmentPoint= 0[00h]);
            AtelOp.PUSHII  .build(0x0003),
            AtelOp.PUSHII  .build(0x0000),
            AtelOp.CALLPOPA.build(0x8009),
            // AE0300 AE0100 D80280    call Map.setGfxActive? [8002h](gfxIndex= 3[03h], active= true[01h]);
            AtelOp.PUSHII  .build(0x0003),
            AtelOp.PUSHII  .build(0x0001),
            AtelOp.CALLPOPA.build(0x8002),
            // AE1E00 D80000           call Common.wait[0000h] (frames=30 [1Eh]);
            AtelOp.PUSHII  .build(0x001E),
            AtelOp.CALLPOPA.build(0x0000),
            // AE3C00 D88801           call Common.0188(p1=60 [3Ch]);
            AtelOp.PUSHII  .build(0x003C),
            AtelOp.CALLPOPA.build(0x0188),
            // AE0F00 D80540           call SgEvent.fadeoutToBlack?[4005h] (frames=15 [0Fh]);
            AtelOp.PUSHII  .build(0x000F),
            AtelOp.CALLPOPA.build(0x4005),
            // AE0200 AE0100 D88400    call Common.waitForText [0084h](boxIndex= 2[02h], p2= 1[01h]);
            AtelOp.PUSHII  .build(0x0002),
            AtelOp.PUSHII  .build(0x0001),
            AtelOp.CALLPOPA.build(0x0084),
            // AE0F00 D80000           call Common.wait[0000h] (frames=15 [0Fh]);
            AtelOp.PUSHII  .build(0x000F),
            AtelOp.CALLPOPA.build(0x0000),
            // AE0000 A00100           Set IsBetweenDjoseFaythTalkAndAirshipBoarding = false [00h];
            AtelOp.PUSHII  .build(0x0000),
            AtelOp.POPV    .build(0x0001),
            // Variable location depends on event script. Injected into slot 1 here

            // call Common.00BB(0);
            AtelOp.PUSHII  .build(0x0000),
            AtelOp.CALLPOPA.build(0x00BB),
            // call Common.00BC(0);
            AtelOp.PUSHII  .build(0x0000),
            AtelOp.CALLPOPA.build(0x00BC),
            // call Common.warpToMap?[010Bh](382, 0); (Airship Menu)
            AtelOp.PUSHII  .build(   382),
            AtelOp.PUSHII  .build(     0),
            AtelOp.CALLPOPA.build(0x010B),
            // Jump to j02 (51A8)
            AtelOp.JMP     .build(2),
        ]).SelectMany(x => x.to_bytes()).ToArray(),

        // bvyt0900 Save Sphere (Play Blitzball option)
        ((AtelInst[])[ // 8
            // AE0500 A20400 AE0600 0B D72E00                   Check (BlitzballTeamPlayerCount[Besaid Aurochs [05h]] < 6 [06h]) else jump to j2E (2D22)
            AtelOp.PUSHII   .build(0x0005),
            AtelOp.PUSHAR   .build(0x0001),
            AtelOp.PUSHII   .build(0x0006),
            AtelOp.LS       .build(      ),
            //AtelOp.POPXNCJMP.build(0x0000),
            // call Common.JumpIfFalse(value, 9) i.e. if !value jump to customscripts[9]
            AtelOp.PUSHII  .build(0x0009),
            AtelOp.CALLPOPA.build((ushort)CustomCallTarget.JUMP_IF_FALSE),
            // AE0200 AE0100 D88400                             call Common.waitForText [0084h](boxIndex=2 [02h], p2=1 [01h]);
            AtelOp.PUSHII  .build(0x0002),
            AtelOp.PUSHII  .build(0x0001),
            AtelOp.CALLPOPA.build(0x0084),
            // AD3900 D8DF00                                    call Common.playSfx[00DFh] (sfx=sfx:-2147483645 [80000003h]);
            .. atelPushInt(0x80000003),
            AtelOp.CALLPOPA.build(0x00DF),
            // AE0100 AE0001 AEE000 AE0400 D86500               call Common.positionText [0065h](boxIndex= 1[01h], x= 256[0100h], y= 224[E0h], align= Center[04h]);
            // AE0100 AE0000 D86600                             call Common.setTextHasTransparentBackdrop [0066h](boxIndex= 1[01h], transparent= false[00h]);
            // AE0100 AE1300 D86400                             call Common.displayFieldString [0064h](boxIndex= 1[01h], string= "{TIME:00}The {MCR:s06l42:"Besaid Aurochs"} are not at full{\n}strength. You need more members{\n}to participate in blitzball." [13h]);
            // AE0100 AE0000 D89D00                             call Common.setTextFlags[009Dh] (boxIndex=1 [01h], textFlags=[] [00h]);
            // AE0100 AE0000 D86A00                             call Common.006A(boxIndex= 1[01h], p2= 0[00h]);
            .. atelDisplayFieldString(1, 0x8004, 256, 224, 4, 0, 0),
            // AE0100 AE0100 D88400                             call Common.waitForText[0084h] (boxIndex=1 [01h], p2=1 [01h]);
            AtelOp.PUSHII  .build(0x0001),
            AtelOp.PUSHII  .build(0x0001),
            AtelOp.CALLPOPA.build(0x0084),
            // B02500                                           Jump to j22(2D86)
            AtelOp.JMP     .build(2),
        ]).SelectMany(x => x.to_bytes()).ToArray(),

        // bvyt0900 Save Sphere (Play Blitzball option)
        ((AtelInst[])[ // 9
            // AD3A00 D8DF00                                    call Common.playSfx [00DFh](sfx=BoardAirship [80000048h]);
            .. atelPushInt(0x80000048),
            AtelOp.CALLPOPA.build(0x00DF),
            // AE0300 AE0000 D80980                             call Map.bindGfxToTarget[8009h] (gfxIndex=3 [03h], attachmentPoint=0 [00h]);
            AtelOp.PUSHII  .build(0x0003),
            AtelOp.PUSHII  .build(0x0000),
            AtelOp.CALLPOPA.build(0x8009),
            // AE0300 AE0100 D80280                             call Map.setGfxActive?[8002h] (gfxIndex=3 [03h], active=true [01h]);
            AtelOp.PUSHII  .build(0x0003),
            AtelOp.PUSHII  .build(0x0001),
            AtelOp.CALLPOPA.build(0x8002),
            // AE1E00 D80000                                    call Common.wait[0000h] (frames=30 [1Eh]);
            AtelOp.PUSHII  .build(0x001E),
            AtelOp.CALLPOPA.build(0x0000),
            // AE0F00 D80540                                    call SgEvent.fadeoutToBlack? [4005h](frames= 15[0Fh]);
            AtelOp.PUSHII  .build(0x000F),
            AtelOp.CALLPOPA.build(0x4005),
            // AE0200 AE0100 D88400                             call Common.waitForText[0084h] (boxIndex=2 [02h], p2=1 [01h]);
            AtelOp.PUSHII  .build(0x0002),
            AtelOp.PUSHII  .build(0x0001),
            AtelOp.CALLPOPA.build(0x0084),
            // AE0F00 D80000                                    call Common.wait [0000h](frames= 15[0Fh]);
            AtelOp.PUSHII  .build(0x000F),
            AtelOp.CALLPOPA.build(0x0000),
            // AE0000 D8BB00                                    call Common.00BB(p1= 0[00h]);
            AtelOp.PUSHII  .build(0x0000),
            AtelOp.CALLPOPA.build(0x00BB),
            // AE0000 D8BC00                                    call Common.00BC(p1= 0[00h]);
            AtelOp.PUSHII  .build(0x0000),
            AtelOp.CALLPOPA.build(0x00BC),
            // AE5B01 AE0000 D80B01                             call Common.warpToMap?[010Bh] (map=bltz0200[015Bh], entranceIndex?=0 [00h]);
            AtelOp.PUSHII  .build(0x015B),
            AtelOp.PUSHII  .build(0x0000),
            AtelOp.CALLPOPA.build(0x010B),
            // 3C                                               return;
            AtelOp.RET     .build(      ),
        ]).SelectMany(x => x.to_bytes()).ToArray(),

        // Post-Yu Yevon battle
        ((AtelInst[])[ // A
            // call Common.disablePlayerControl? [005Eh]();
            AtelOp.CALLPOPA.build(0x005e),
            // call Common.displayFieldChoice [013Bh](boxIndex=1 [01h], string=customString[5], p3=0 [00h], p4=1 [01h], x=280 [0118h], y=224 [E0h], align=Center [04h]);
            AtelOp.PUSHII  .build(0x0001),
            AtelOp.PUSHII  .build(0x8005),
            AtelOp.PUSHII  .build(0x0000),
            AtelOp.PUSHII  .build(0x0001),
            AtelOp.PUSHII  .build(0x0100),
            AtelOp.PUSHII  .build(0x00e0),
            AtelOp.PUSHII  .build(0x0004),
            AtelOp.CALLPOPA.build(0x013b),
            AtelOp.PUSHA.build(),
            // call Common.waitForText [0084h](boxIndex=1 [01h], p2=1 [01h]);
            AtelOp.PUSHII  .build(0x0001),
            AtelOp.PUSHII  .build(0x0001),
            AtelOp.CALLPOPA.build(0x0084),
            // call Common.enablePlayerControl? [005Dh]();
            AtelOp.CALLPOPA.build(0x005d),

            // call Common.JumpIfFalse(choice, 11) i.e. if !choice jump to customscripts[B]
            AtelOp.PUSHII  .build(0x000B),
            AtelOp.CALLPOPA.build((ushort)CustomCallTarget.JUMP_IF_FALSE),


            // AE0000 D8BB00        call Common.00BB(p1=0 [00h]);
            AtelOp.PUSHII  .build(0x0000),
            AtelOp.CALLPOPA.build(0x00bb),
            // AE0000 D8BC00        call Common.00BC(p1=0 [00h]);
            AtelOp.PUSHII  .build(0x0000),
            AtelOp.CALLPOPA.build(0x00bc),
            // AE8201 AE0000 D80B01 call Common.warpToMap? [010Bh](map=endg0100 (Besaid Village) [0182h], entranceIndex?=0 [00h]);
            AtelOp.PUSHII  .build(0x0182),
            AtelOp.PUSHII  .build(0x0000),
            AtelOp.CALLPOPA.build(0x010b),
            AtelOp.RET     .build(      ),

        ]).SelectMany(x => x.to_bytes()).ToArray(),

        // Ending skip
        ((AtelInst[])[ // B
            // AEF82A A00000 Set GameMoment = 11000 [2AF8h];
            AtelOp.PUSHII  .build(0x2AF8),
            AtelOp.POPV    .build(0x0000),
            AtelOp.RET     .build(      ),

        ]).SelectMany(x => x.to_bytes()).ToArray(),

        // Check Nemesis requirements
        ((AtelInst[])[ // C

            // !!!(MonsterArenaOriginalCreationUnlockFlags[0] & 128 [80h])
            AtelOp.PUSHII   .build(0x0000),
            AtelOp.PUSHAR   .build(0x000C),
            AtelOp.PUSHII   .build(0x0080),
            AtelOp.AND      .build(),
            AtelOp.NOT      .build(),
            AtelOp.NOT      .build(),
            AtelOp.NOT      .build(),

            // (priv175A5 >= 13 [0Dh])
            AtelOp.PUSHV    .build(0x001F),
            AtelOp.PUSHII   .build(0x000D),
            AtelOp.GTE      .build(),

            // Common.?checkMultipleMonsterArenaUnlocks [023Ch](from=Creation #1 defeated [0300h], next=34 [22h])
            AtelOp.PUSHII   .build(0x0300),
            AtelOp.PUSHII   .build(0x0022),
            AtelOp.CALL     .build(0x023C),

            // else jump to jAD8 (DCED)
            AtelOp.LAND     .build(),
            AtelOp.LAND     .build(),
            AtelOp.POPXNCJMP.build(0x0AD8),

            // if Custom.isTreasureLocationChecked(0x01CA) jump by 9
            AtelOp.PUSHII   .build(0x01CA),
            AtelOp.CALL     .build((ushort)CustomCallTarget.IS_TREASURE_LOCATION_CHECKED),
            AtelOp.PUSHII   .build(0x0009),
            AtelOp.CALLPOPA .build((ushort)CustomCallTarget.OFFSET_IF_TRUE),

            // call Common.obtainTreasure [015Bh](msgWindow=0 [00h], treasure=Item: 10x Master Sphere [2050h] [01CAh]);
            AtelOp.PUSHII   .build(0x0000),
            AtelOp.PUSHII   .build(0x01CA),
            AtelOp.CALLPOPA .build(0x015B),

            // Custom.isGoalUnlocked
            AtelOp.CALL     .build((ushort)CustomCallTarget.IS_GOAL_UNLOCKED),

            // if Custom.isGoalUnlocked() jump to jAD7 (D7B3)
            AtelOp.POPXCJMP .build(0x0AD7),

            // display customStrings[2]
            .. atelDisplayFieldString(1, 0x8002, 256, 224, 4, 0, 0),

            // call Common.waitForText [0084h](boxIndex=1 [01h], p2=1 [01h]);
            AtelOp.PUSHII   .build(0x0001),
            AtelOp.PUSHII   .build(0x0001),
            AtelOp.CALLPOPA .build(0x0084),

            // jump to jAD8 (DCED)
            AtelOp.JMP      .build(0x0AD8),

        ]).SelectMany(x => x.to_bytes()).ToArray(),
    };

    private static readonly AtelInst[] save_sphere_load_model = [
        AtelOp.PUSHII  .build(0x5001),
        AtelOp.CALLPOPA.build(0x0001),
    ];


    private static readonly AtelInst[] save_sphere_load_underwater_model = [
        AtelOp.PUSHII  .build(0x50AB),
        AtelOp.CALLPOPA.build(0x0001),
    ];

    private static readonly Dictionary<string, (uint hasCelestial, uint hasCloudy, uint celestialObtained)> event_to_celestial_offsets = new(){
        {"bjyt0200", (0x02EA9, 0x02F3F, 0x02FE8)},
        {"kami0000", (0x0459F, 0x04632, 0x046D8)},
        {"kino0000", (0x10AF4, 0x10B87, 0x10C2D)},
        {"kino0100", (0x0398D, 0x03A20, 0x03AC6)},
        {"luca0400", (0x08F45, 0x08F74, 0x08FB6)},
        {"nagi0000", (0x3420F, 0x342A2, 0x34348)},
        {"nagi0700", (0x0E357, 0x0E3EA, 0x0E490)},
    };

    private static readonly Dictionary<string, (uint, int)[]> event_to_primer_offsets = new(){
        {"azit0300", [(0x03236, 20)] },
        {"azit0600", [(0x00EC7, 19)] },
        {"bika0000", [(0x011AD,  0), (0x01688,  2)] }, // Also a !check at 0x15FC
        {"bika0100", [(0x01C3D,  4), (0x020F3, 13), (0x025AD, 13)] },
        {"bika0200", [(0x00FA9, 16), (0x0142A, 17)] },
        {"bika0400", [(0x02689, 18)] },
        {"bsvr0200", [(0x0935E,  1)] },
        {"bvyt0400", [(0x01174, 21)] },
        {"cdsp0000", [(0x0C1E0,  0)] }, // Also a !check at 0xC62F
        {"genk1200", [(0x01A4D, 11)] },
        {"guad0400", [(0x0361A, 12)] },
        {"kami0100", [(0x04502, 13), (0x49BC, 13)] },
        {"kino0400", [(0x07678, 10)] },
        {"kino0500", [(0x0767E,  9)] },
        {"lchb1100", [(0x027F3,  6)] },
        {"lchb1400", [(0x01CEB,  5)] },
        {"lmyt0000", [(0x05532, 23)] },
        {"maca0000", [(0x044AD, 15)] },
        {"mcfr0400", [(0x0299A, 14)] },
        {"mihn0300", [(0x04042,  7)] },
        {"mihn0500", [(0x03C36,  8)] },
        {"nagi0000", [(0x1CD23, 22)] },
        {"nagi0500", [(0x05E96, 24)] },
        {"omeg0000", [(0x060EF, 25)] },
        {"ptkl0800", [(0x026E5,  3)] },
        {"slik0800", [(0x00A05,  2)] },
        {"swin0300", [(0x00B57,  4)] },
        {"swin0900", [(0x00A27,  4)] },
    };

    // Recruit ids are the same as the blitzball player ids in-game
    private static readonly Dictionary<string, (uint offset, ushort recruit_id)[]> event_to_recruit_offsets = new() {
        { "bsvr0400", [ (0x25EC, 52) ] }, // Vilucha

        { "ptkl0200", [
            (0x3391, 15), // Vuroja
            (0x4E95, 13), // Larbeight
            (0x6538, 38), // Tatts
        ] },

        { "ptkl0600", [
            (0x14EB, 13), // Larbeight
            (0x2AC1, 38), // Tatts
            (0x4125, 15), // Vuroja
        ] },

        { "ptkl0800", [ (0x1394, 16) ] }, // Kulukan

        { "ptkl1700", [ (0x154C, 14) ] }, // Isken

        { "ptkl1800", [ (0x154C, 14) ] }, // Isken

        { "klyt0500", [
            (0x23B3, 18), // Nizarut
            (0x3973, 17), // Deim
        ] },

        { "klyt0600", [ (0x12C4, 46) ] }, // Mep

        { "swin0000", [ (0x8981, 56) ] }, // Kiyuri

        { "lchb0000", [
            (0x1C46, 42), // Biggs
            (0x34F8, 43), // Wedge
        ] },

        { "lchb0100", [ (0x41A6, 41) ] }, // Nedus

        { "lchb0300", [
            (0x42B2,  7), // Bickson
            (0x5AA5,  9), // Graav
            (0x7298,  8), // Abus
            (0x8D0B, 10), // Doram
            (0xA4FE, 11), // Balgerda
            (0xBCF1, 12), // Raudy
        ] },

        { "lchb0400", [
            (0x2D5F, 25), // Basik Ronso
            (0x442E, 26), // Argai Ronso
            (0x5AFD, 27), // Gazna Ronso
            (0x71CC, 28), // Nuvy Ronso
            (0x889B, 29), // Irga Ronso
            (0x9F6A, 30), // Zamzi Ronso
        ] },

        { "lchb0500", [ (0x35EC, 54) ] }, // Zev Ronso

        { "lchb0900", [ (0x1F58, 47) ] }, // Zalitz

        { "lchb1300", [
            (0x447A, 2), // Datto
            (0x631B, 3), // Letty
            (0x80DE, 4), // Jassu
            (0x9FDA, 5), // Botta
            (0xBD16, 6), // Keepa
        ] },

        { "lchb1800", [ (0x4407, 53) ] }, // Shaami

        { "luca0100", [ (0x6DB9, 50) ] }, // Jumal

        { "luca0400", [ (0x4402, 40) ] }, // Shuu

        { "mihn0300", [ (0x8A4A, 44) ] }, // Ropp

        { "djyt0000", [ (0x400B, 39) ] }, // Kyou

        { "genk1100", [ (0x13D0, 59) ] }, // Miyu

        { "guad0000", [
            (0x842E, 31), // Giera Guado
            (0x9BD9, 34), // Auda Guado
            (0xB877, 33), // Nav Guado
        ] },

        { "guad0100", [ (0x239A, 36) ] }, // Noy Guado

        { "guad0300", [ (0x20FA, 55) ] }, // Yuma Guado

        { "guad0400", [
            (0x1AD8, 32), // Zazi Guado
            (0x31A6, 35), // Pah Guado
        ] },

        { "kami0100", [ (0x9A8F, 58) ] }, // Mifurey

        { "mcyt0000", [ (0x3B7D, 45) ] }, // Linna

        { "nagi0000", [
            ( 0x8872, 51), // Svanda
            (0x33DDD, 48), // Naida
        ] },

        { "nagi0400", [ (0x2711, 49) ] }, // Durren

        { "hiku0000", [ (0x5906, 37) ] }, // Rin

        { "hiku0500", [
            ( 0x7499, 37), // Rin
            ( 0x8F22, 19), // Eigaar
            ( 0xA8D3, 20), // Blappa
            ( 0xC644, 21), // Berrik
            ( 0xDF7A, 22), // Judda
            ( 0xF940, 23), // Lakkam
            (0x113AD, 24), // Nimrook
        ] },

        { "hiku0800", [
            (0x57EC,  1), // Wakka
            (0xCAA0, 57), // Brother
        ] },

        { "hiku0801", [
            (0x34DB,  1), // Wakka
            (0x687F, 57), // Brother
        ] },

        { "hiku1900", [
            (0x23C4,  1), // Wakka
            (0x6843, 57), // Brother
        ] },
    };

    private static readonly Dictionary<string, (uint offset, ushort southNorth)[]> event_to_lightning_dodging_offsets = new(){
        {"kami0000", [(0x3848, 0)] },
        {"kami0300", [(0x685F, 1)] },
    };

    private static readonly Dictionary<string, (uint offset, ushort other_id, ushort sphere_bit)> event_to_jecht_sphere_offsets = new() {
     // {"        ", (0x0000, 177, 0) }, // Jecht Sphere 1  - Macalania Woods Post-Spherimorph | Handled from story_check 1470
        {"bsvr0000", (0x12F21, 27, 0) }, // Jecht Sphere 2  - Besaid Village Beside Temple
        {"kami0000", (0x7128,  28, 1) }, // Jecht Sphere 3  - Thunder Plains South
        {"genk0600", (0x013B,  29, 2) }, // Jecht Sphere 4  - Moonflow South Warf
        {"mihn0400", (0x4EC5,  30, 3) }, // Jecht Sphere 5  - Miihen Highroad Old-Road
        {"lchb1300", (0x013B,  31, 4) }, // Jecht Sphere 6  - Luca Stadium Basement A
        {"slik0300", (0x103C,  32, 5) }, // Jecht Sphere 7  - S.S. Liki Bridge
        {"mtgz0100", (0x5098,  33, 6) }, // Braska Sphere   - Mt. Gagazet Mountain Trail
        {"mcfr0000", (0x5FFA,  34, 7) }, // Jecht Sphere 8  - Macalania Woods Entrance
        {"kino0500", (0x7BEF,  35, 8) }, // Auron Sphere    - Mushroom Rock Road Ridge
    };

    private static Dictionary<(int, int), uint> originalEntryPoints = new();
    private static string current_event_name = "";
    private void AtelEventSetUp(int event_id) {
        FhXCall.AtelEventSetUp.chain_from(AtelEventSetUp).fnptr!(event_id);

        foreach (NativeCustomString customString in cached_strings) {
            customString.Free();
        }
        cached_strings.Clear();

        originalEntryPoints.Clear();

        foreach ((string key, CustomStringDrawInfo drawInfo) in customStringDrawInfos) {
            if (!drawInfo.persistent) customStringDrawInfos.Remove(key);
        }

        string event_name = Marshal.PtrToStringAnsi((nint)get_event_name((uint)event_id))!;
        _logger.Debug($"atel_event_setup: {event_name}");
        byte* code_ptr = Globals.Atel.controllers[0].worker(0)->code_ptr;
        switch (event_name) {
            case "bjyt1200":
                // Send Anima
                set(code_ptr, 0x1FB4, [
                    AtelOp.PUSHII   .build(13),
                    AtelOp.CALLPOPA .build((ushort)CustomCallTarget.SEND_PARTY_MEMBER_LOCATION),
                    ]);
                break;
            case "slik0000":
                // Set destination to Besaid
                ((byte*)((int)save_data + 0x0C7A))->set_bits<byte>(0, 2, 2);
                break;
            case "swin0000":
                // Set destination to Kilika
                ((byte*)((int)save_data + 0x0C7A))->set_bits<byte>(2, 2, 8);
                break;
            case "ptkl0600":
                set(code_ptr, 0x4D3D, [
                    AtelOp.CALLPOPA.build((ushort)CustomCallTarget.BLOCK_KILIKA_BOAT_CHOICE),
                    .. atelNOPArray(1),
                ]);
                break;
            case "hiku2100":
                _logger.Debug($"atel_event_setup: Inject set_airship_destinations call");
                set(code_ptr, 0x26D1, [
                    .. atelNOPArray(3),
                    AtelOp.CALLPOPA.build((ushort)CustomCallTarget.SET_AIRSHIP_DESTINATIONS),
                    ]);

                set(code_ptr, 0x4028, AtelOp.JMP.build(0x013A));


                // Disable showtexture calls
                set(code_ptr, 0x2976, atelNOPArray(10));
                set(code_ptr, 0x34CF, atelNOPArray(10));


                // Show airship destinations
                set(code_ptr, 0x2AC4, [
                    .. atelNOPArray(15),
                    AtelOp.CALLPOPA.build((ushort)CustomCallTarget.SHOW_AIRSHIP_DESTINATIONS),
                    ]);


                // Hide airship destinations
                set(code_ptr, 0x3765, [
                    .. atelNOPArray(7),
                    AtelOp.CALLPOPA.build((ushort)CustomCallTarget.HIDE_AIRSHIP_DESTINATIONS),
                    ]);
                set(code_ptr, 0x3AFA, [
                    .. atelNOPArray(7),
                    AtelOp.CALLPOPA.build((ushort)CustomCallTarget.HIDE_AIRSHIP_DESTINATIONS),
                    ]);
                set(code_ptr, 0x47FB, [
                    .. atelNOPArray(7),
                    AtelOp.CALLPOPA.build((ushort)CustomCallTarget.HIDE_AIRSHIP_DESTINATIONS),
                    ]);
                set(code_ptr, 0x3C37, [
                    .. atelNOPArray(3),
                    AtelOp.CALLPOPA.build((ushort)CustomCallTarget.HIDE_AIRSHIP_DESTINATIONS),
                    ]);


                // Show current airship position
                set(code_ptr, 0x193F, [
                    .. atelNOPArray(3),
                    AtelOp.CALLPOPA.build((ushort)CustomCallTarget.SHOW_CURRENT_AIRSHIP_LOCATION),
                    ]);
                // Hide current airship position
                set(code_ptr, 0x3C07, [
                    .. atelNOPArray(3),
                    AtelOp.CALLPOPA.build((ushort)CustomCallTarget.HIDE_CURRENT_AIRSHIP_LOCATION),
                    ]);
                set(code_ptr, 0x47FB, [
                    .. atelNOPArray(7),
                    AtelOp.CALLPOPA.build((ushort)CustomCallTarget.HIDE_CURRENT_AIRSHIP_LOCATION),
                    ]);

                set(code_ptr,
                    [
                        0x1CB8 + 3, 0x1CDF + 3, 0x1D06 + 3, 0x1D2D + 3,
                        0x1D54 + 3, 0x1D7B + 3, 0x1DA2 + 3, 0x1DC9 + 3,
                        0x1DF0 + 3, 0x1E17 + 3, 0x1E3E + 3, 0x1E65 + 3,
                        0x1E8C + 3, 0x1EB3 + 3, 0x1EDA + 3, 0x1F01 + 3,
                        0x1F28 + 3, 0x1F4F + 3, 0x1F76 + 3, 0x1F9D + 3,
                        0x1FC4 + 3, 0x1FEB + 3, 0x2012 + 3, 0x2039 + 3,
                    ],
                    AtelOp.PUSHII.build(0)
                    );

                set(code_ptr, 0x434E, AtelOp.PUSHII.build(12)); // Change destination 26 switch case to 12

                set(code_ptr, 0x4028, atelNOPArray(13));         // Skip setting GameMoment for destination 16
                set(code_ptr, 0x4118, AtelOp.JMP.build(0x0141)); // Skip setting GameMoment for destination 18

                // Replace Penance destination with Monster Arena
                //set(code_ptr, 0x434E, AtelOp.PUSHII.build(18)); // Replace switch check
                //set(code_ptr, 0x426F, AtelOp.PUSHII.build(307)); // Replace destination map

                // Backing out returns to menu
                set(code_ptr, [0x4041, 0x4903, 0x491B, 0x4933], [
                                AtelOp.PUSHII  .build(382),
                                AtelOp.PUSHII  .build(0),
                            ]);

                foreach ((ArchipelagoData.RegionEnum region, ArchipelagoData.ArchipelagoRegion state) in region_states) {
                    if (state.airship_destination_index != 99) {
                        uint address = airship_destination_addresses[state.airship_destination_index - 1];
                        set(code_ptr, address, [
                                .. atelNOPArray(3),
                                AtelOp.PUSHII  .build((ushort)region),
                                AtelOp.CALLPOPA.build((ushort)CustomCallTarget.TRANSITION_TO_REGION),
                            ]);
                    }
                }

                break;
            case "hiku0801":
                _logger.Debug($"atel_event_setup: Inject Cid talk hook");
                set(code_ptr, 0x4DC5, [
                    AtelOp.PUSHII  .build(0x0000),
                    AtelOp.CALLPOPA.build((ushort)CustomCallTarget.JUMP), // Common.Jump(0000) = jump to customScripts[0]
                ]);
                break;
            case "hiku0500":
                // Rin collected primers check inventory instead of count
                set(code_ptr, 0x5869, AtelOp.CALL.build((ushort)CustomCallTarget.COLLECTED_PRIMERS));
                break;
            case "ssbt0300":
                _logger.Debug($"atel_event_setup: Redirect Overdrive Sin post-battle warp");
                set(code_ptr, 0x500E, AtelOp.PUSHII.build(382));
                break;
            case "sins0700":
                _logger.Debug($"atel_event_setup: Handle removing Aeons");

                // Lock all Aeons and skip Contest of Aeons
                if (seed.Options.SkipContestOfAeons == 1) {
                    set(code_ptr, 0x58FA, [
                        AtelOp.CALLPOPA.build((ushort)CustomCallTarget.LOCK_ALL_AEONS),
                        AtelOp.JMP.build(0x0003),
                        ]);
                } else {
                    set(code_ptr, 0x58FA, [
                        AtelOp.CALL.build((ushort)CustomCallTarget.CHECK_UNLOCKED_AEONS),
                        ]);
                }

                set(code_ptr, 0x5972, [
                    AtelOp.PUSHII  .build(0x0002),
                    AtelOp.CALLPOPA.build((ushort)CustomCallTarget.JUMP), // Common.Jump(2) = jump to customScripts[2]
                    ]);

                // Ending skip choice
                set(code_ptr, 0x7391, [
                    AtelOp.PUSHII  .build(0x000A),
                    AtelOp.CALLPOPA.build((ushort)CustomCallTarget.JUMP), // Common.Jump(A) = jump to customScripts[A]
                    ]);
                break;
            case "luca0400":
                _logger.Debug($"atel_event_setup: Wait longer");
                set(code_ptr, 0x68F9, [
                    AtelOp.PUSHII  .build(10),
                    AtelOp.CALLPOPA.build( 0),
                    ]);
                break;
            case "kino0000":
                set(code_ptr, 0xAE24 + 10, [
                    AtelOp.PUSHII  .build(382),
                    ]);
                break;
            case "sins0900":
                // Check requirements if Yu Yevon is goal (Tower falling down)
                if (seed.Options.Goal == ArchipelagoData.Goal.YuYevon) {
                    Globals.Atel.current_controller->worker(0xD)->table_jump[0] = 0x1951;
                    AtelBasicWorker* _0xd = Globals.Atel.current_controller->worker(0xD);
                    set(code_ptr, 0x1947, [
                        AtelOp.PUSHII  .build(0x0003),
                        AtelOp.CALLPOPA.build((ushort)CustomCallTarget.JUMP), // Common.Jump(3) = jump to customScripts[3]
                        ]);
                }

                // Look up at tower
                //set(code_ptr, 0x1761, [
                //    AtelOp.RET.build(),
                //    ]);
                break;
            case "genk0000":
                // Remove GameMoment check for Belgemine
                set(code_ptr, 0x2180 + 8, atelNOPArray(8));

                // Remove loss and soul rewards
                set(code_ptr, 0x28AB, atelNOPArray(18));

                // Remove Common.halt()
                set(code_ptr, 0x7678, atelNOPArray(3));

                // Move section forward by 24 bytes
                Globals.Atel.current_controller->worker(0x10)->table_jump[0x06] = 0x7594 + 24 + 16; // vanilla+24+16
                Globals.Atel.current_controller->worker(0x10)->table_jump[0x07] = 0x75E6 + 16; // vanilla+16
                move(code_ptr, 0x7594, 24, 0x75B6 - 0x7594);
                move(code_ptr, 0x7594 + 24, 16, 0x75F4 - (0x7594 + 24));

                // Don't always set flag
                //set(code_ptr, 0x75F4, atelNOPArray(10));

                // Don't always hide Belgemine
                set(code_ptr, 0x763B, atelNOPArray(11));

                // Set flag and hide Belgemine on win (15 + 24 + 16 bytes available)
                set(code_ptr, 0x758B, [
                    // AE3C00 D80000                   call Common.wait [0000h](frames=90);
                    AtelOp.PUSHII  .build(90),
                    AtelOp.CALLPOPA.build(0x0000),

                    // 9F0500 AE0200 03 A00500         Set MoonflowFlags3 = MoonflowFlags3 | 2 [02h];
                    AtelOp.PUSHV   .build(0x0005),
                    AtelOp.PUSHII  .build(0x0002),
                    AtelOp.OR      .build(),
                    AtelOp.POPV    .build(0x0005),

                    // AE0B00 D83600                   call Common.stopMotion [0036h](worker=w0B [0Bh]);
                    AtelOp.PUSHII  .build(0x000B),
                    AtelOp.CALLPOPA.build(0x0036),

                    // AE0300 AE0B00 AE0700 38 25      runAndAwaitEnd w0Be07 (Level 3);
                    AtelOp.PUSHII  .build(0x0003),
                    AtelOp.PUSHII  .build(0x000B),
                    AtelOp.PUSHII  .build(0x0007),
                    AtelOp.REQEW   .build(),
                    AtelOp.POPA    .build(),

                    // AE0B00 D8A700                   call Common.deactivate [00A7h](worker=w0B [0Bh]);
                    AtelOp.PUSHII  .build(0x000B),
                    AtelOp.CALLPOPA.build(0x00A7),

                    AtelOp.JMP     .build(0x0007),
                    //.. atelNOPArray(3),
                    ]);
                break;
            case "mcfr0100":
                // Check Saturn Sigil location instead of inventory (butterfly minigame)
                set(code_ptr, [0x5792, 0x5985, 0x5AD5], [
                    AtelOp.PUSHII   .build(277),
                    AtelOp.CALL     .build((ushort)CustomCallTarget.IS_TREASURE_LOCATION_CHECKED),
                    ]);

                // Remove NPC
                set(code_ptr, 0x1601, [
                    AtelOp.JMP.build(0x0001),
                    ]);
                // Disable invisible wall
                set(code_ptr, 0x5C8B, atelNOPArray(28));
                // Check for (has_cloudy && !CelestialMirrorObtained) instead of (has_cloudy && !has_celestial)
                set(code_ptr, 0x7A5C, [
                    AtelOp.PUSHI    .build(0x0000),
                    AtelOp.CALL     .build(0x0160),

                    AtelOp.PUSHV    .build(0x0001),
                    AtelOp.PUSHII   .build(0x0000),
                    AtelOp.EQ       .build(),
                    AtelOp.LAND     .build(),

                    // Disable side quest requirement
                    .. atelNOPArray(13),
                    ]);

                // Don't remove Cloudy Mirror
                set(code_ptr, 0x197B, atelNOPArray(6));
                // Don't remove Cloudy Mirror
                set(code_ptr, 0x7C3B, atelNOPArray(6));

                // Check inventory instead of flags for Celestial weapons and also check for Celestial Mirror
                set(code_ptr, 0x7AA4, [
                    AtelOp.PUSHII   .build(PlySaveId.PC_TIDUS),
                    AtelOp.CALL     .build((ushort)CustomCallTarget.HAS_CELESTIAL),

                    AtelOp.PUSHII   .build(PlySaveId.PC_YUNA),
                    AtelOp.CALL     .build((ushort)CustomCallTarget.HAS_CELESTIAL),
                    AtelOp.LOR      .build(),

                    AtelOp.PUSHII   .build(PlySaveId.PC_AURON),
                    AtelOp.CALL     .build((ushort)CustomCallTarget.HAS_CELESTIAL),
                    AtelOp.LOR      .build(),

                    AtelOp.PUSHII   .build(PlySaveId.PC_KIMAHRI),
                    AtelOp.CALL     .build((ushort)CustomCallTarget.HAS_CELESTIAL),
                    AtelOp.LOR      .build(),

                    AtelOp.PUSHII   .build(PlySaveId.PC_WAKKA),
                    AtelOp.CALL     .build((ushort)CustomCallTarget.HAS_CELESTIAL),
                    AtelOp.LOR      .build(),

                    AtelOp.PUSHII   .build(PlySaveId.PC_LULU),
                    AtelOp.CALL     .build((ushort)CustomCallTarget.HAS_CELESTIAL),
                    AtelOp.LOR      .build(),

                    AtelOp.PUSHII   .build(PlySaveId.PC_RIKKU),
                    AtelOp.CALL     .build((ushort)CustomCallTarget.HAS_CELESTIAL),
                    AtelOp.LOR      .build(),

                    // Common.hasKeyItem [0160h](keyItem=Celestial Mirror [0000A003h])
                    AtelOp.PUSHII   .build(0xA003),
                    AtelOp.CALL     .build(0x0160),
                    AtelOp.LAND     .build(),

                    .. atelNOPArray(35),
                    ]);

                set(code_ptr, 0x7EF7, [
                    AtelOp.PUSHII   .build(PlySaveId.PC_TIDUS),
                    AtelOp.CALL     .build((ushort)CustomCallTarget.HAS_CELESTIAL),
                    AtelOp.NOT      .build(),

                    .. atelNOPArray(6),
                    ]);
                set(code_ptr, 0x9774, [
                    AtelOp.PUSHII   .build(PlySaveId.PC_YUNA),
                    AtelOp.CALL     .build((ushort)CustomCallTarget.HAS_CELESTIAL),
                    AtelOp.NOT      .build(),

                    .. atelNOPArray(6),
                    ]);
                set(code_ptr, 0xAFF1, [
                    AtelOp.PUSHII   .build(PlySaveId.PC_AURON),
                    AtelOp.CALL     .build((ushort)CustomCallTarget.HAS_CELESTIAL),
                    AtelOp.NOT      .build(),

                    .. atelNOPArray(6),
                    ]);
                set(code_ptr, 0xC86E, [
                    AtelOp.PUSHII   .build(PlySaveId.PC_KIMAHRI),
                    AtelOp.CALL     .build((ushort)CustomCallTarget.HAS_CELESTIAL),
                    AtelOp.NOT      .build(),

                    .. atelNOPArray(6),
                    ]);
                set(code_ptr, 0xE0EB, [
                    AtelOp.PUSHII   .build(PlySaveId.PC_WAKKA),
                    AtelOp.CALL     .build((ushort)CustomCallTarget.HAS_CELESTIAL),
                    AtelOp.NOT      .build(),

                    .. atelNOPArray(6),
                    ]);
                set(code_ptr, 0xF968, [
                    AtelOp.PUSHII   .build(PlySaveId.PC_LULU),
                    AtelOp.CALL     .build((ushort)CustomCallTarget.HAS_CELESTIAL),
                    AtelOp.NOT      .build(),

                    .. atelNOPArray(6),
                    ]);
                set(code_ptr, 0x111E5, [
                    AtelOp.PUSHII   .build(PlySaveId.PC_RIKKU),
                    AtelOp.CALL     .build((ushort)CustomCallTarget.HAS_CELESTIAL),
                    AtelOp.NOT      .build(),

                    .. atelNOPArray(6),
                    ]);

                break;
            case "lmyt0100":
                // Yojimbo fight
                set(code_ptr, 0x441A, [
                    AtelOp.PUSHII   .build(PlySaveId.PC_YOJIMBO),
                    AtelOp.CALL     .build((ushort)CustomCallTarget.IS_CHARACTER_UNLOCKED),

                    .. atelNOPArray(6),
                    ]);
                // Anima fight
                set(code_ptr, 0x4509, [
                    AtelOp.PUSHII   .build(PlySaveId.PC_ANIMA),
                    AtelOp.CALL     .build((ushort)CustomCallTarget.IS_CHARACTER_UNLOCKED),

                    .. atelNOPArray(6),
                    ]);
                // Magus Sisters fight
                set(code_ptr, 0x4614, [
                    AtelOp.PUSHII   .build(PlySaveId.PC_MAGUS1),
                    AtelOp.CALL     .build((ushort)CustomCallTarget.IS_CHARACTER_UNLOCKED),

                    .. atelNOPArray(6),
                    ]);

                // ????
                set(code_ptr, 0x57D4, [
                    //AtelOp.PUSHII   .build(PlySaveId.PC_YOJIMBO),
                    //AtelOp.CALL     .build((ushort)CustomCallTarget.IS_CHARACTER_UNLOCKED),

                    //AtelOp.PUSHII   .build(PlySaveId.PC_ANIMA),
                    //AtelOp.CALL     .build((ushort)CustomCallTarget.IS_CHARACTER_UNLOCKED),
                    //AtelOp.LAND     .build(),
                    .. atelNOPArray(13),

                    AtelOp.PUSHII   .build(0x0000),
                    AtelOp.PUSHAR   .build(0x0005),
                    AtelOp.PUSHII   .build(0x0010),
                    AtelOp.AND      .build(      ),
                    AtelOp.NOT      .build(      ),
                    AtelOp.NOT      .build(      ),
                    AtelOp.LAND     .build(      ),

                    AtelOp.PUSHII   .build(0x0000),
                    AtelOp.PUSHAR   .build(0x0005),
                    AtelOp.PUSHII   .build(0x0020),
                    AtelOp.AND      .build(      ),
                    AtelOp.NOT      .build(      ),
                    AtelOp.NOT      .build(      ),
                    AtelOp.LAND     .build(      ),

                    .. atelNOPArray(12),
                    ]);

                set(code_ptr, [0x5870, 0x5937], [
                    //AtelOp.PUSHII   .build(PlySaveId.PC_YOJIMBO),
                    //AtelOp.CALL     .build((ushort)CustomCallTarget.IS_CHARACTER_UNLOCKED),

                    //AtelOp.PUSHII   .build(PlySaveId.PC_ANIMA),
                    //AtelOp.CALL     .build((ushort)CustomCallTarget.IS_CHARACTER_UNLOCKED),
                    //AtelOp.LAND     .build(),

                    AtelOp.PUSHII.build(1),
                    .. atelNOPArray(22),
                    ]);

                // Send Magus Sisters
                set(code_ptr, 0x5B9C, [
                    AtelOp.PUSHII  .build(15), // stack [15]
                    AtelOp.CALLPOPA.build((ushort)CustomCallTarget.SEND_PARTY_MEMBER_LOCATION),
                    ]);
                break;
            case "bltz0200":
                // Remove min battle requirement for Blitzball prizes
                set(code_ptr, 0xCCDB, atelNOPArray(10));
                set(code_ptr, 0xE686, atelNOPArray(10));
                break;
            case "mihn0000":
                // Ignore GameMoment for Belgemine
                set(code_ptr, 0x8346, [
                    // AE0000 A21100 AE0200 05 19 19 19    !!!(BelgemineFightProgressionFlags[0] & 2 [02h])
                    AtelOp.PUSHII  .build(0x0000),
                    AtelOp.PUSHAR  .build(0x0011),
                    AtelOp.PUSHII  .build(0x0002),
                    AtelOp.AND     .build(),
                    AtelOp.NOT     .build(),
                    AtelOp.NOT     .build(),
                    AtelOp.NOT     .build(),
                    ..atelNOPArray(8),
                    ]);

                // Remove loss reward
                set(code_ptr, [0x8934, 0x8E7C], atelNOPArray(9));

                // Move back by 6
                move(code_ptr, 0x8895, -6, 0x88D8 - 0x8895);
                move(code_ptr, 0x8DDD, -6, 0x8E20 - 0x8DDD);
                // Move back by 14
                move(code_ptr, 0x86D8, -14, 0x88D8 - 0x86D8);
                move(code_ptr, 0x8C20, -14, 0x8E20 - 0x8C20);

                foreach ((uint offset, ushort elseJump) in ((uint, ushort)[])[(0x88D8, 0x0D), (0x8E20, 0x1B)]) {
                    set(code_ptr, offset - 14 - 6, [

                        // Check (Battle.BattleEndType() == 2) else jump to elseJump
                        AtelOp.CALL     .build(0x7035),
                        AtelOp.PUSHII   .build(0x0002),
                        AtelOp.EQ       .build(),
                        AtelOp.POPXNCJMP.build(elseJump),

                        // AE0000 2B A21100 AE0200 03 A31100    Set BelgemineFightProgressionFlags[0] = BelgemineFightProgressionFlags[0] | 2 [02h];
                        AtelOp.PUSHII   .build(0x0000),
                        AtelOp.REPUSH   .build(),
                        AtelOp.PUSHAR   .build(0x0011),
                        AtelOp.PUSHII   .build(0x0002),
                        AtelOp.OR       .build(),
                        AtelOp.POPAR    .build(0x0011),

                        ..atelNOPArray(3),
                        ]);
                }

                // Don't unload Belgemine
                move(code_ptr, 0x8AF0, 0x8ADF - 0x8AF0, 3);
                Globals.Atel.current_controller->worker(0x18)->table_jump[0x0C] = 0x8ADF + 3;
                set(code_ptr, 0x893D, AtelOp.JMP.build(0x0C));

                move(code_ptr, 0x9038, 0x9027 - 0x9038, 3);
                Globals.Atel.current_controller->worker(0x18)->table_jump[0x1A] = 0x9027 + 3;
                set(code_ptr, 0x8E85, AtelOp.JMP.build(0x1A));


                break;
            case "mihn0700":
                set(code_ptr, 0x4E78, [
                    AtelOp.CALLPOPA.build((ushort)CustomCallTarget.KICKED_BLITZBALL_AWAY),
                    ..atelNOPArray(3),
                    ]);
                break;
            case "mcfr0200":
                // Check Saturn Sigil location instead of inventory
                set(code_ptr, [0x2647, 0x283A, 0x298A], [
                    AtelOp.PUSHII   .build(277),
                    AtelOp.CALL     .build((ushort)CustomCallTarget.IS_TREASURE_LOCATION_CHECKED),
                    ]);
                break;
            case "nagi0000":
                // Check Sun Sigil location instead of inventory
                set(code_ptr, [0x21701, 0x227D8, 0x238AF, 0x24986], [
                    AtelOp.PUSHII   .build(274),
                    AtelOp.CALL     .build((ushort)CustomCallTarget.IS_TREASURE_LOCATION_CHECKED),
                    ]);

                // Turn around if locked
                //set(code_ptr, 0x346BE + 6, [
                //    AtelOp.CALLPOPA     .build((ushort)CustomCallTarget.BLOCK_WARP),
                //    ]);
                //set(code_ptr, 0x346A1, atelNOPArray(17));


                // Belgemine
                // Ignore GameMoment and Familiarity
                set(code_ptr, 0x5819, [
                    // AE0000 A21700 AE0800 05 19 19 19    !!!(BelgemineFightProgressionFlags[0] & 8 [08h])
                        AtelOp.PUSHII   .build(0x0000),
                        AtelOp.PUSHAR   .build(0x0017),
                        AtelOp.PUSHII   .build(0x0008),
                        AtelOp.AND      .build(),
                        AtelOp.NOT      .build(),
                        AtelOp.NOT      .build(),
                        AtelOp.NOT      .build(),
                        .. atelNOPArray(17),
                    ]);

                // Remove loss and soul rewards
                set(code_ptr, [0x5FC7, 0x674B, 0x6CC1], atelNOPArray(18));

                // Move code blocks back by 14
                //swap(code_ptr, 0x5D19, 0x5D0B, 0x5E96 - 0x5D19);
                //swap(code_ptr, 0x649D, 0x648F, 0x661A - 0x649D);
                //swap(code_ptr, 0x6A13, 0x6A05, 0x6B90 - 0x6A13);
                move(code_ptr, 0x5D19, -14, 0x5E96 - 0x5D19);
                move(code_ptr, 0x649D, -14, 0x661A - 0x649D);
                move(code_ptr, 0x6A13, -14, 0x6B90 - 0x6A13);

                foreach ((uint offset, ushort elseJump) in ((uint, ushort)[])[(0x5E96, 0x10), (0x661A, 0x1F), (0x6B90, 0x2D)]) {
                    set(code_ptr, offset - 14, [
                        // AE3C00 AE3000 D88001    call Common.changeCurrentBgmVol? [0180h](oldVol?=60 [3Ch], newVol?=48 [30h]);
                        AtelOp.PUSHII   .build(0x003C),
                        AtelOp.PUSHII   .build(0x0030),
                        AtelOp.CALLPOPA .build(0x0180),

                        // Check (Battle.BattleEndType() == 2) else jump to elseJump
                        AtelOp.CALL     .build(0x7035),
                        AtelOp.PUSHII   .build(0x0002),
                        AtelOp.EQ       .build(),
                        AtelOp.POPXNCJMP.build(elseJump),

                        // AE0000 2B A21700 AE0800 03 A31700    Set BelgemineFightProgressionFlags[0] = BelgemineFightProgressionFlags[0] | 8 [08h];
                        AtelOp.PUSHII   .build(0x0000),
                        AtelOp.REPUSH   .build(),
                        AtelOp.PUSHAR   .build(0x0017),
                        AtelOp.PUSHII   .build(0x0008),
                        AtelOp.OR       .build(),
                        AtelOp.POPAR    .build(0x0017),

                        ..atelNOPArray(3),
                        ]);
                }

                // Don't unload Belgemine
                set(code_ptr, [0x611E, 0x68A2, 0x6E18], atelNOPArray(3));

                Globals.Atel.current_controller->worker(0x0A)->table_jump[0x0F] = 0x60E4;
                set(code_ptr, 0x6029, AtelOp.JMP.build(0x0F));

                Globals.Atel.current_controller->worker(0x0A)->table_jump[0x1E] = 0x6868;
                set(code_ptr, 0x67AD, AtelOp.JMP.build(0x1E));

                Globals.Atel.current_controller->worker(0x0A)->table_jump[0x2C] = 0x6DDE;
                set(code_ptr, 0x6D23, AtelOp.JMP.build(0x2C));


                break;
            case "nagi0400":
                set(code_ptr, [0x4402, 0x597D], [
                    AtelOp.JMP.build(0x0000),
                    .. atelNOPArray(13),
                    ]);
                break;
            case "nagi0600":
                // Send Yojimbo
                set(code_ptr, [0x261A, 0x2686, 0x28FB, 0x2B20, 0x2D45, 0x2F39], [
                    AtelOp.PUSHII  .build(14),
                    AtelOp.CALLPOPA.build((ushort)CustomCallTarget.SEND_PARTY_MEMBER_LOCATION),
                    ]);
                break;
            case "nagi0700":
                // Check requirements if Nemesis is goal
                if (seed.Options.Goal == ArchipelagoData.Goal.Nemesis) {
                    Globals.Atel.current_controller->worker(0x02)->table_jump[0xAD7] = 0xD7B3;
                    set(code_ptr, 0xD78F, [
                        AtelOp.PUSHII  .build(0x000C),
                        AtelOp.CALLPOPA.build((ushort)CustomCallTarget.JUMP),
                        ]);

                    // Remove obtainTreasure call
                    set(code_ptr, 0xD81D, atelNOPArray(9));
                }
                break;

        }

        // Blitz Recruit locations (RecruitSanity)
        if (event_to_recruit_offsets.TryGetValue(event_name, out var recruit_offsets)) {
            foreach ((uint offset, ushort recruit_id) in recruit_offsets) {
                set(code_ptr, offset, [
                    AtelOp.PUSHII   .build(recruit_id),
                    AtelOp.CALL     .build((ushort)CustomCallTarget.SEND_RECRUIT_LOCATION),
                    AtelOp.NOP      .build()
                    ]);
            }
        }

        //Lightning Dodging
        if (event_to_lightning_dodging_offsets.TryGetValue(event_name, out var lightning_offset)) {
            // kami0000 - 0x3848 | kami0300 - 0x685F
            foreach ((uint offset, ushort southNorth) in lightning_offset) {
                set(code_ptr, offset, [
                    AtelOp.PUSHII       .build(southNorth),
                    AtelOp.CALL         .build((ushort)CustomCallTarget.LIGHTNING_DODGING),
                    AtelOp.NOP          .build(),
                    ]);
            }
        }

        // Jecht Sphere locations
        if (event_to_jecht_sphere_offsets.TryGetValue(event_name, out var jecht_sphere)) {
            set(code_ptr, jecht_sphere.offset, [
                AtelOp.PUSHII   .build(jecht_sphere.other_id),
                AtelOp.PUSHII   .build(jecht_sphere.sphere_bit),
                AtelOp.CALLPOPA .build((ushort)CustomCallTarget.JECHT_SPHERE),
                AtelOp.JMP      .build(0x0005),
                .. atelNOPArray(4),
                ]);
        }

        // Celestial weapon locations
        if (event_to_celestial_offsets.TryGetValue(event_name, out var celestial_offsets)) {
            // Remove CelestialMirrorObtained check
            set(code_ptr, celestial_offsets.hasCelestial + 6, atelNOPArray(8));
            set(code_ptr, celestial_offsets.hasCloudy + 6, atelNOPArray(8));
            set(code_ptr, celestial_offsets.celestialObtained, [
                AtelOp.PUSHII   .build(0xA003),
                AtelOp.CALL     .build(0x0160),
                AtelOp.NOP      .build(),
                ]);
        }

        if (event_to_primer_offsets.TryGetValue(event_name, out var primer_offsets)) {
            foreach ((uint offset, int primer) in primer_offsets) {
                set(code_ptr, offset, [
                    AtelOp.PUSHII   .build((ushort)(primer+1)),
                    AtelOp.CALL     .build((ushort)CustomCallTarget.IS_OTHER_LOCATION_CHECKED),
                    AtelOp.NOP      .build(),
                    ]);
            }
            {
                uint? offset = null;
                if (event_name == "bika0000") offset = 0x15FC;
                if (event_name == "cdsp0000") offset = 0xC62F;
                if (offset.HasValue) {
                    set(code_ptr, offset.Value, [
                        AtelOp.PUSHII   .build(1),
                        AtelOp.CALL     .build((ushort)CustomCallTarget.IS_OTHER_LOCATION_CHECKED),
                        AtelOp.NOT      .build(),
                        AtelOp.NOP      .build(),
                        ]);
                }
            }
        }

        // Inject save sphere hook
        if (event_name == "nagi0000") {
            uint save_sphere_offset = 0x1BB69;
            _logger.Info($"Save sphere init at {save_sphere_offset}");
            set(code_ptr, save_sphere_offset + 0x48, AtelOp.JMP.build(0x0007)); // Always all options

            // Update region state. Also skips save sphere tutorial
            set(code_ptr, save_sphere_offset + 0x59B, [
                AtelOp.CALLPOPA.build((ushort)CustomCallTarget.UPDATE_REGION_STATE),
                AtelOp.JMP     .build(0x0023),
                ]);

            // Board Airship option
            set(code_ptr, save_sphere_offset + 0x695, [
                // call Common.00BB(0);
                AtelOp.PUSHII  .build(0x0000),
                AtelOp.CALLPOPA.build(0x00BB),
                // call Common.00BC(0);
                AtelOp.PUSHII  .build(0x0000),
                AtelOp.CALLPOPA.build(0x00BC),
                // call Common.warpToMap?[010Bh](382, 0); (Airship Menu)
                AtelOp.PUSHII  .build(   382),
                AtelOp.PUSHII  .build(     0),
                AtelOp.CALLPOPA.build(0x010B),
                ]);

        }
        else if (event_name == "cdsp0700") {
            uint save_sphere_offset = 0x2AA3;
            _logger.Info($"Underwater save sphere init at {save_sphere_offset}");
            set(code_ptr, save_sphere_offset + 0x5A, AtelOp.JMP.build(0x0002)); // Always all options

            // Update region state. Also skips save sphere tutorial
            set(code_ptr, save_sphere_offset + 0x542, [
                AtelOp.CALLPOPA.build((ushort)CustomCallTarget.UPDATE_REGION_STATE),
                AtelOp.JMP     .build(0x001D),
                ]);

            // Board Airship option
            set(code_ptr, save_sphere_offset + 0x628, [
                // call Common.00BB(0);
                AtelOp.PUSHII  .build(0x0000),
                AtelOp.CALLPOPA.build(0x00BB),
                // call Common.00BC(0);
                AtelOp.PUSHII  .build(0x0000),
                AtelOp.CALLPOPA.build(0x00BC),
                // call Common.warpToMap?[010Bh](382, 0); (Airship Menu)
                AtelOp.PUSHII  .build(   382),
                AtelOp.PUSHII  .build(     0),
                AtelOp.CALLPOPA.build(0x010B),
                ]);

        }
        else if (event_name == "stbv0000") {
            uint save_sphere_offset = 0x2982;
            _logger.Info($"Underwater save sphere init at {save_sphere_offset}");
            set(code_ptr, save_sphere_offset + 0x5A, AtelOp.JMP.build(0x0002)); // Always all options

            // Update region state. Also skips save sphere tutorial
            set(code_ptr, save_sphere_offset + 0x542, [
                AtelOp.CALLPOPA.build((ushort)CustomCallTarget.UPDATE_REGION_STATE),
                AtelOp.JMP     .build(0x001D),
                ]);

            // Board Airship option
            set(code_ptr, save_sphere_offset + 0x628, [
                // call Common.00BB(0);
                AtelOp.PUSHII  .build(0x0000),
                AtelOp.CALLPOPA.build(0x00BB),
                // call Common.00BC(0);
                AtelOp.PUSHII  .build(0x0000),
                AtelOp.CALLPOPA.build(0x00BC),
                // call Common.warpToMap?[010Bh](382, 0); (Airship Menu)
                AtelOp.PUSHII  .build(   382),
                AtelOp.PUSHII  .build(     0),
                AtelOp.CALLPOPA.build(0x010B),
                ]);


            save_sphere_offset = 0x3168;
            _logger.Info($"Underwater save sphere init at {save_sphere_offset}");
            set(code_ptr, save_sphere_offset + 0x5A, AtelOp.JMP.build(0x0002)); // Always all options

            // Update region state. Also skips save sphere tutorial
            set(code_ptr, 0x3746, [
                AtelOp.CALLPOPA.build((ushort)CustomCallTarget.UPDATE_REGION_STATE),
                AtelOp.JMP     .build(0x001E),
                ]);

            // Board Airship option
            set(code_ptr, 0x382C, [
                // call Common.00BB(0);
                AtelOp.PUSHII  .build(0x0000),
                AtelOp.CALLPOPA.build(0x00BB),
                // call Common.00BC(0);
                AtelOp.PUSHII  .build(0x0000),
                AtelOp.CALLPOPA.build(0x00BC),
                // call Common.warpToMap?[010Bh](382, 0); (Airship Menu)
                AtelOp.PUSHII  .build(   382),
                AtelOp.PUSHII  .build(     0),
                AtelOp.CALLPOPA.build(0x010B),
                ]);
        }
        else if (false && event_name == "stbv0100") {
            uint save_sphere_offset = 0xF07F;
            _logger.Info($"Save sphere init at {save_sphere_offset}");
            set(code_ptr, save_sphere_offset + 0x48, AtelOp.JMP.build(0x0007)); // Always all options

            // Update region state. Also skips save sphere tutorial
            set(code_ptr, save_sphere_offset + 0x571, [
                AtelOp.CALLPOPA.build((ushort)CustomCallTarget.UPDATE_REGION_STATE),
                AtelOp.JMP     .build(0x0023),
                ]);

            // Board Airship option
            set(code_ptr, save_sphere_offset + 0x657, [
                // call Common.00BB(0);
                AtelOp.PUSHII  .build(0x0000),
                AtelOp.CALLPOPA.build(0x00BB),
                // call Common.00BC(0);
                AtelOp.PUSHII  .build(0x0000),
                AtelOp.CALLPOPA.build(0x00BC),
                // call Common.warpToMap?[010Bh](382, 0); (Airship Menu)
                AtelOp.PUSHII  .build(   382),
                AtelOp.PUSHII  .build(     0),
                AtelOp.CALLPOPA.build(0x010B),
                ]);


            save_sphere_offset = 0xFCF1;
            _logger.Info($"Save sphere init at {save_sphere_offset}");
            set(code_ptr, save_sphere_offset + 0x48, AtelOp.JMP.build(0x0007)); // Always all options

            // Update region state. Also skips save sphere tutorial
            set(code_ptr, 0x10278, [
                AtelOp.CALLPOPA.build((ushort)CustomCallTarget.UPDATE_REGION_STATE),
                AtelOp.JMP     .build(0x0025),
                ]);

            // Board Airship option
            set(code_ptr, 0x1035E, [
                // call Common.00BB(0);
                AtelOp.PUSHII  .build(0x0000),
                AtelOp.CALLPOPA.build(0x00BB),
                // call Common.00BC(0);
                AtelOp.PUSHII  .build(0x0000),
                AtelOp.CALLPOPA.build(0x00BC),
                // call Common.warpToMap?[010Bh](382, 0); (Airship Menu)
                AtelOp.PUSHII  .build(   382),
                AtelOp.PUSHII  .build(     0),
                AtelOp.CALLPOPA.build(0x010B),
                ]);

        }
        else if (event_name == "bvyt0900") {

            //int save_sphere_offset = 0x507B;
            uint tutorial_offset    = 0x7E;
            uint menu_string_offset = 0xD8;
            foreach (uint save_sphere_offset in (uint[])[0x507B, 0x5253]) {
                set(code_ptr, save_sphere_offset + tutorial_offset, [
                    AtelOp.CALLPOPA.build((ushort)CustomCallTarget.UPDATE_REGION_STATE),
                    AtelOp.JMP     .build(0x0000),
                    ]);

                set(code_ptr, save_sphere_offset + menu_string_offset + 3, AtelOp.PUSHII.build(0x8003)); // Set to customString[3]
            }

            AtelBasicWorker* save_sphere_worker_1 = Globals.Atel.current_controller->worker(0x13);
            // Custom switch
            save_sphere_worker_1->table_jump[1] = (uint)(customScriptHandles[6].AddrOfPinnedObject() - (nint)save_sphere_worker_1->code_ptr);
            _logger.Debug($"{save_sphere_worker_1->table_var[1].raw}");
            save_sphere_worker_1->table_var[1].raw = 0x0000000100000A98;

            AtelBasicWorker* save_sphere_worker_2 = Globals.Atel.current_controller->worker(0x14);
            // Custom switch
            save_sphere_worker_2->table_jump[1] = (uint)(customScriptHandles[6].AddrOfPinnedObject() - (nint)save_sphere_worker_2->code_ptr);
        }
        else {
            AtelInst? previous_op = null;
            AtelInst? current_op = null;
            uint code_length = Globals.Atel.controllers[0].worker(0)->script_chunk->code_length;
            uint i = 0;
            uint save_spheres_detected = 0;
            uint sphere_level_offset = 0x48;
            uint tutorial_offset = 0x571;
            ushort tutorial_jump = 0x23;
            uint airship_warp_offset = 0x657;
            if (event_name == "mihn0000" || event_name == "mihn0200" || event_name == "mihn0300" || event_name == "mihn0400" || event_name == "mihn0600") {
                tutorial_offset = 0x59B;
                airship_warp_offset = 0x695;
            }
            while (i < code_length) {
                previous_op = current_op;
                AtelOp inst = (AtelOp)code_ptr[i];
                if (inst.has_operand()) {
                    current_op = inst.build(*(ushort*)(code_ptr + i + 1));
                }
                else {
                    current_op = inst.build();
                }
                if (current_op == save_sphere_load_model[1] && previous_op == save_sphere_load_model[0]) {
                    if (save_spheres_detected == 1) {
                        // Additional save spheres usually have different offsets
                        sphere_level_offset = 0x48;
                        tutorial_offset = 0x587;
                        tutorial_jump = 0x25;
                        airship_warp_offset = 0x66D;
                    }
                    else if (save_spheres_detected > 1) {
                        // Potentially incorrect offsets
                        _logger.Info("Potentially incorrect Save Sphere offsets");
                    }
                    save_spheres_detected++;
                    _logger.Info($"Detected save sphere init at {i - 6}");
                    uint save_sphere_offset = i - 6;

                    AtelOp someInst = (AtelOp)code_ptr[save_sphere_offset + sphere_level_offset];
                    if (!someInst.has_operand() || someInst.build(*(ushort*)(code_ptr + save_sphere_offset + sphere_level_offset + 1)) != AtelOp.PUSHV.build(0x0000)) {
                        _logger.Warning($"Unexpected instruction at {save_sphere_offset + sphere_level_offset}");
                    }
                    set(code_ptr, save_sphere_offset + sphere_level_offset, AtelOp.JMP.build(0x0007)); // Always all options



                    someInst = (AtelOp)code_ptr[save_sphere_offset + tutorial_offset + 13];
                    if (!someInst.has_operand() || someInst.build(*(ushort*)(code_ptr + save_sphere_offset + tutorial_offset + 13 + 1)) != AtelOp.POPXNCJMP.build(tutorial_jump)) {
                        _logger.Warning($"Unexpected instruction at {save_sphere_offset + tutorial_offset + 13}");
                    }
                    // Update region state. Also skips save sphere tutorial
                    set(code_ptr, save_sphere_offset + tutorial_offset, [
                        AtelOp.CALLPOPA.build((ushort)CustomCallTarget.UPDATE_REGION_STATE),
                        AtelOp.JMP     .build(tutorial_jump),
                        ]);


                    someInst = (AtelOp)code_ptr[save_sphere_offset + airship_warp_offset];
                    if (!someInst.has_operand() || someInst.build(*(ushort*)(code_ptr + save_sphere_offset + airship_warp_offset + 1)) != AtelOp.PUSHV.build(0x0000)) {
                        _logger.Warning($"Unexpected instruction at {save_sphere_offset + airship_warp_offset}");
                    }
                    // Board Airship option
                    set(code_ptr, save_sphere_offset + airship_warp_offset, [
                        // call Common.00BB(0);
                        AtelOp.PUSHII  .build(0x0000),
                        AtelOp.CALLPOPA.build(0x00BB),
                        // call Common.00BC(0);
                        AtelOp.PUSHII  .build(0x0000),
                        AtelOp.CALLPOPA.build(0x00BC),
                        // call Common.warpToMap?[010Bh](382, 0); (Airship Menu)
                        AtelOp.PUSHII  .build(   382),
                        AtelOp.PUSHII  .build(     0),
                        AtelOp.CALLPOPA.build(0x010B),
                        ]);
                }

                i += (uint)(inst.has_operand() ? 3 : 1);
            }
        }

        current_event_name = event_name;
    }

    private void Common_obtainTreasureInit(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        int treasure_id = atelStack->values.as_int()[1];
        _logger.Info($"obtain_treasure: {treasure_id}");
        //FhXCall.Common_obtainTreasureInit.chain_from(h_Common_obtainTreasureInit).fnptr!(work, storage, atelStack);
        obtainTreasureInitReimplement(work, storage, atelStack);
    }


    private void obtainTreasureInitReimplement(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        byte* item_name = (byte*)0;

        int treasure_id = atelStack->pop_int();
        int window_id = atelStack->pop_int();
        _logger.Debug($"window_id:{window_id}, treasure_id:{treasure_id}");

        FhXCall.SndSepPlaySimple.fnptr!(0x80000026);
        AtelWorkerController* pAVar2 = (AtelWorkerController*)FhXCall.AtelGetCurCtrlWork.fnptr!();
        ((byte*)pAVar2)[3] |= 4;
        FhXCall.MsFieldItemGet.fnptr!(treasure_id);
        FhXCall.FUN_008b8910.fnptr!(window_id, 0, 0);
        FhXCall.FUN_008b8910.fnptr!(window_id, 1, 1);
        bool gear_inv_is_full = false;
        uint weapon_id = 0;
        byte* message_text = FhXCall.TkBtlEndGetText.fnptr!(0x401d); // "Nothing"

        if (item_locations.treasure.TryGetValue(treasure_id, out var item)) {
            if (_client!.sendLocation(treasure_id, ArchipelagoClientModule.ArchipelagoLocationType.Treasure)) {
                obtain_item(item.id);

                NativeCustomString name = new NativeCustomString(item.id != 0 ? item.name : $"{item.name} to {item.player}", encodingFlags: FhEncodingFlags.IGNORE_EXPRESSIONS);
                //CustomString name = new CustomString(item.name, encodingFlags: FhEncodingFlags.IGNORE_EXPRESSIONS);
                _logger.Info(item.name);

                cached_strings.Add(name);
                item_name = name.encoded;
                if (item.id != 0) {
                    message_text = FhXCall.TkBtlEndGetText.fnptr!(0x4018); // "Obtained %0!"
                } else {
                    NativeCustomString sent_text = new NativeCustomString("Sent {VAR:00}!");
                    //CustomString sent_text = new CustomString("Sent {VAR:00} to {VAR:01}!");
                    cached_strings.Add(sent_text);
                    message_text = sent_text.encoded;

                    //CustomString player_name = new CustomString(item.player, encodingFlags: FhEncodingFlags.IGNORE_EXPRESSIONS);
                    //cached_strings.Add(player_name);
                    //_FUN_008b8930(window_id, 1, (int)player_name.encoded);
                }
            } else {
                NativeCustomString sent_text = new NativeCustomString("Already received this item!");
                cached_strings.Add(sent_text);
                message_text = sent_text.encoded;
            }
        }
        else
        if (Battle.reward_data->item_count != 0) {
            FhXCall.TkMsGetRomItem.fnptr!(Battle.reward_data->items[0], (int*)&item_name);
            FhXCall.FUN_008b8930.fnptr!(window_id, 1, Battle.reward_data->items_amounts[0]);
            if (Battle.reward_data->items_amounts[0] == 1) {
                message_text = FhXCall.TkBtlEndGetText.fnptr!(0x4018); // "Obtained %0!"
            }
            else {
                message_text = FhXCall.TkBtlEndGetText.fnptr!(0x4019); // "Obtained %0 x%1!"
            }

            byte[] decoded = new byte[FhEncoding.compute_decode_buffer_size(new ReadOnlySpan<byte>(item_name, 1000))];

            int decoded_length = FhEncoding.decode(new ReadOnlySpan<byte>(item_name, 1000), decoded, flags:FhEncodingFlags.IMPLICIT_END);
            //string decoded = FhEncoding.Us.to_string(item_name);
            _logger.Info(Encoding.UTF8.GetString(decoded, 0, decoded_length));

            FhXCall.MsSaveItemUse.fnptr!(Battle.reward_data->items[0], Battle.reward_data->items_amounts[0]);
        }
        else if (Battle.reward_data->key_item_count != 0) {
            item_name = FhXCall.MsImportantName.fnptr!(Battle.reward_data->key_item);
            message_text = FhXCall.TkBtlEndGetText.fnptr!(0x4017); // "Obtained %0!"
            FhXCall.TkMsImportantSet.fnptr!(Battle.reward_data->key_item);
        }
        else if (Battle.reward_data->gear_count != 0) {
            weapon_id = Battle.reward_data->gear_inv_idx;
            message_text = FhXCall.TkBtlEndGetText.fnptr!(0x4016); // "Obtained %0!"
            Equipment* weapon = (Equipment*)FhXCall.MsGetSaveWeapon.fnptr!(weapon_id, (nint)(&item_name));
            int inv_id = FhXCall.FUN_007ab930.fnptr!(weapon); // giveWeapon?
            gear_inv_is_full = inv_id == 0;
        }
        else if (Battle.reward_data->gil != 0) {
            FhXCall.FUN_008b8930.fnptr!(window_id, 1, (int)Battle.reward_data->gil);
            message_text = FhXCall.TkBtlEndGetText.fnptr!(0x401a); // "Obtained %1 Gil!"
            FhXCall.MsPayGIL.fnptr!(-(int)Battle.reward_data->gil);
        }

        FhXCall.FUN_008b8930.fnptr!(window_id, 0, (int)item_name);

        atelStack->push_int(window_id);
        atelStack->push_int(0x100);
        atelStack->push_int(0xd0);
        atelStack->push_int(4);
        h_CT_RetInt_0065.fnptr!(work, storage, atelStack);

        TOMesWinWork* mesageWindowWorker = FhXCall.AtelGetMesWinWork.fnptr!(window_id);
        mesageWindowWorker->_0x20 = 0;
        mesageWindowWorker->text = message_text;
        mesageWindowWorker->_0xc = message_text;
        mesageWindowWorker->_0x16 = 0;
        mesageWindowWorker->_0x18 = 0;

        atelStack->push_int(window_id);
        atelStack->push_int(0);
        h_CT_RetInt_006A.fnptr!(work, storage, atelStack);

        *storage = window_id;
        storage[1] = 1;
        storage[2] = gear_inv_is_full ? 1 : 0;
        storage[3] = (int)weapon_id;

        if (!gear_inv_is_full) {
            FhXCall.MsBtlGetInit.fnptr!();
        }
        FhXCall.FUN_0086a0c0.fnptr!();
        mesageWindowWorker->_0x1d |= 0x10;
    }

    private void Common_obtainTreasureSilentlyInit(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        int treasure_id = atelStack->values.as_int()[0];
        _logger.Info($"obtain_treasure_silently: {treasure_id}");
        //FhXCall.Common_obtainTreasureSilentlyInit.chain_from(h_Common_obtainTreasureSilentlyInit).fnptr!(work, storage, atelStack);
        obtainTreasureSilentlyInitReimplement(work, storage, atelStack);
    }

    private void obtainTreasureSilentlyInitReimplement(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {

        int treasure_id = atelStack->pop_int();
        FhXCall.MsFieldItemGet.fnptr!(treasure_id);
        bool gear_inv_is_full = false;
        uint weapon_id = 0;

        if (item_locations.treasure.TryGetValue(treasure_id, out var item)) {
            if (_client!.sendLocation(treasure_id, ArchipelagoClientModule.ArchipelagoLocationType.Treasure)) {
                obtain_item(item.id);
            }
        }
        else
        if (Battle.reward_data->item_count != 0) {
            FhXCall.MsSaveItemUse.fnptr!(Battle.reward_data->items[0], Battle.reward_data->items_amounts[0]);
        }
        else if (Battle.reward_data->key_item_count != 0) {
            FhXCall.TkMsImportantSet.fnptr!(Battle.reward_data->key_item);
        }
        else if (Battle.reward_data->gear_count != 0) {
            weapon_id = Battle.reward_data->gear_inv_idx;
            Equipment* weapon = FhXCall.MsGetSaveWeapon.fnptr!(weapon_id, 0);
            int inv_id = FhXCall.FUN_007ab930.fnptr!(weapon); // giveWeapon?
            gear_inv_is_full = inv_id == 0;
        }
        else if (Battle.reward_data->gil != 0) {
            FhXCall.MsPayGIL.fnptr!(-(int)Battle.reward_data->gil);
        }

        storage[2] = gear_inv_is_full ? 1 : 0;
        storage[3] = (int)weapon_id;

        if (!gear_inv_is_full) {
            FhXCall.MsBtlGetInit.fnptr!();
        }
    }

    private int Common_obtainBrotherhoodRetInt(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        _logger.Debug("obtain_brotherhoodRetInt");
        if (atelStack->size > 0) {
            throw new Exception($"Too many parameters ({atelStack->size}) passed to obtainBrotherhood!");
        }
        if (item_locations.other.TryGetValue(0, out var item)) {
            obtain_item(item.id);
            return 1;
        }
        return h_Common_obtainBrotherhoodRetInt.chain_from(Common_obtainBrotherhoodRetInt).fnptr!(work, storage, atelStack);
    }
    private int Common_upgradeBrotherhoodRetInt(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        _logger.Debug("upgrade_brotherhoodRetInt");
        if (item_locations.other.TryGetValue(37, out var item)) {
            obtain_item(item.id);
            return 1;
        }

        return h_Common_upgradeBrotherhoodRetInt.chain_from(Common_upgradeBrotherhoodRetInt).fnptr!(work, storage, atelStack);
    }
    private int Common_isBrotherhoodUnpoweredRetInt(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        _logger.Debug("isBrotherhoodUnpoweredRetInt");

        return h_CT_RetInt_01B6.chain_from(Common_isBrotherhoodUnpoweredRetInt).fnptr!(work, storage, atelStack);
    }
    //private static int h_Common_grantCelestialUpgrade(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
    //    int character = atelStack->values.as_int()[0];
    //    int level = atelStack->values.as_int()[1];
    //    _logger.Debug($"grant_celestial_upgrade: character={id_to_character[character]}, level={level}");
    //
    //
    //    if (0 <= character && character <= 6 && item_locations.other.TryGetValue(36 + level + character*2, out var item)) {
    //        obtain_item(item.id);
    //        atelStack->pop_int();
    //        atelStack->pop_int();
    //        return level;
    //    }
    //
    //    return _Common_grantCelestialUpgrade.orig_fptr(work, storage, atelStack);
    //}

    private int TkSetLegendAbility(int chr_id, int level) {
        _logger.Debug($"grant_celestial_upgrade: character={id_to_character[chr_id]}, level={level}");

        int other_id = 37 + level + chr_id * 2;
        if (0 <= chr_id && chr_id <= 6 && item_locations.other.TryGetValue(other_id, out var item)) {
            if (_client!.sendLocation(other_id, ArchipelagoClientModule.ArchipelagoLocationType.Other)) {
                obtain_item(item.id);
            }
            return level;
        }

        return FhXCall.TkSetLegendAbility.chain_from(TkSetLegendAbility).fnptr!(chr_id, level);
    }


    private int Common_setPrimerCollected(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        int primer = atelStack->values.as_int()[0];
        _logger.Debug($"set_primer_collected: Al Bhed Primer {primer + 1}");

        if (item_locations.other.TryGetValue(primer + 1, out var item)) {
            if (_client!.sendLocation(primer + 1, ArchipelagoClientModule.ArchipelagoLocationType.Other)) {
                obtain_item(item.id);
            }
            atelStack->pop_int();
            return 1;
        }

        return h_Common_setPrimerCollected.chain_from(Common_setPrimerCollected).fnptr!(work, storage, atelStack);
    }

    private void update_region_state() {
        update_region_state(save_data->current_room_id, save_data->current_spawnpoint);
    }
    private void update_region_state(int map, int entrance) {
        _logger.Info("Update region state");

        // Update region state
        if (current_region != RegionEnum.None && region_states.TryGetValue(current_region, out ArchipelagoRegion? current_state)) {
            current_state = region_states[current_region];

            for (int i = 0; i < current_state.savedata.Length; i++) {
                ref ArchipelagoRegionSaveData data = ref current_state.savedata[i];
                new Span<byte>((byte*)((int)save_data + data.offset), data.size).CopyTo(data.bytes);
            }

            if (skip_state_updates) {
                return;
            }
            current_state.room_id = (ushort)map;
            current_state.entrance = (ushort)entrance;
            current_state.story_progress = save_data->story_progress;
        }
    }

    private void restore_region_state(ref int map, ref int entrance) {
        ArchipelagoRegion current_state = region_states[current_region];
        save_data->story_progress = current_state.story_progress;
        map      = current_state.room_id;
        entrance = (byte)current_state.entrance;

        foreach (var data in current_state.savedata) {
            data.bytes.CopyTo(new Span<byte>((byte*)((int)save_data + data.offset), data.size));
        }
    }

    private void on_map_change() {
        int map      = save_data->current_room_id;
        int entrance = save_data->current_spawnpoint;
        handle_warp_transition(save_data->last_room_id, save_data->last_spawnpoint, ref map, ref entrance); // We can ignore return value?
        if (map == 382) update_region_state(save_data->last_room_id, save_data->last_spawnpoint);
        if (save_data->current_room_id != map) {
            save_data->current_room_id    = (ushort)map;
            save_data->current_spawnpoint = (byte)entrance;
        }
    }
    private bool handle_warp_transition(int current_map, int current_entrance, ref int next_map, ref int next_entrance) {
        if (next_map == -1) {
            // Loading a save
            next_map = save_data->saved_current_room_id;
            next_entrance = save_data->saved_current_spawnpoint;
            save_data->last_room_id = save_data->saved_last_room_id;
            save_data->last_spawnpoint = save_data->saved_last_spawnpoint;
            save_data->saved_current_spawnpoint = 0;
            if (id_to_regions.Contains(next_map)) {
                var regions = id_to_regions[next_map];
                RegionEnum region = regions.Any(r => current_region == r) ? current_region : regions.Last();
                current_region = region;

                restore_region_state(ref next_map, ref next_entrance);
            }
        }

        if (id_to_regions.Contains(next_map)) {
            // New Game
            if (save_data->current_room_id == 0 && next_map == 132) {
                current_region = RegionEnum.DreamZanarkand;
                _client!.local_checked_locations.Clear();
                _client!.received_items = 0;
                _client!.remote_locations_updated = true;
                // Load seed here?
                if (!loadSeed()) {
                    next_map = 23;
                    next_entrance = 0;
                    return handle_warp_transition(current_map, current_entrance, ref next_map, ref next_entrance);
                    //return true;
                }
                foreach (uint item in seed.Locations.StartingItems) obtain_item(item);
                OverdriveModule.OverdriveProvider.set_overdrive_modes();
            }

            if (seed.Options.SeedId is null) {
                // In-game with no seed
                next_map = 23;
                next_entrance = 0;
                return handle_warp_transition(current_map, current_entrance, ref next_map, ref next_entrance);
                //return true;
            }

            var regions = id_to_regions[next_map];
            RegionEnum region = regions.Any(r => current_region == r) ? current_region : regions.Last();

            if (current_region != region) {
                next_map = 382;
                next_entrance = 0;
                return false;
            }

            // Skip crystal collecting
            if (next_map == 324 && save_data->story_progress == 3250) {
                _logger.Info("Skipping crystal collecting");
                next_map = 325;
                next_entrance = 0;
                save_data->story_progress = 3260;
                return handle_warp_transition(current_map, current_entrance, ref next_map, ref next_entrance);
            }
        } else {
            if (next_map == 23) {
                _logger.Debug("Enter main menu");
                // Main Menu
                initalize_states();
                seed = default;
            }

            if (next_map == 382) update_region_state(current_map, current_entrance); // Airship Menu
            current_region = RegionEnum.None;
            skip_state_updates = false;
        }
        return true;
    }

    private void refill_inventory() {
        _logger.Debug("Refill inventory");

        foreach ((var item_id, var amount) in excess_inventory.ToList()) {
            if (amount > 0 && save_data->get_item_count((int)item_id) < 99) {
                excess_inventory[item_id] = 0;
                give_item(item_id, amount);
            }
        }
    }

    private int Common_transitionToMap(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        ref int map      = ref atelStack->values.as_int()[0];
        ref int entrance = ref atelStack->values.as_int()[1];
        _logger.Debug($"transition_to_map: map={map}, entrance={entrance}");

        if (handle_warp_transition(save_data->current_room_id, save_data->current_spawnpoint, ref map, ref entrance)) {
            return h_Common_transitionToMap.chain_from(Common_transitionToMap).fnptr!(work, storage, atelStack);
        }

        return blockWarp(work, storage, atelStack);
    }
    private int Common_warpToMap(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        ref int map      = ref atelStack->values.as_int()[0];
        ref int entrance = ref atelStack->values.as_int()[1];
        _logger.Debug($"warp_to_map: map={map}, entrance={entrance}");

        // Skip intro
        if (map == 348) {
            _logger.Debug("Skip intro");
            map = 23;
        }

        if (handle_warp_transition(save_data->current_room_id, save_data->current_spawnpoint, ref map, ref entrance)) {
            return h_Common_warpToMap.chain_from(Common_warpToMap).fnptr!(work, storage, atelStack);
        }

        return blockWarp(work, storage, atelStack);
    }

    private void SgEvent_showModularMenuInit(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        int menu = atelStack->values.as_int()[0];
        int unknown1 = menu >> 24 & 0xFF;
        int menuType = menu >> 16 & 0xFF;
        int unknown2 = menu >> 8 & 0xFF;
        int index = menu & 0xFF;

        string menuTypeString = menuType switch {
            0x00 => "Menu",
            0x01 => "BattleRewards.",
            0x02 => "ItemShop",
            0x04 => "WeaponShop",
            0x08 => "EnterName.",
            0x10 => "LoadGame",
            0x20 => "SaveGame",
            0x40 => "SphereMonitor",
            0x80 => "Tutorial",
            int x => $"Unknown({x})",
        };

        // TODO: Investigate why Magus Sisters naming menus (0x4008000F, 0x40080010, 0x40080011) need to be skipped
        if (menu == 0x40800001 || menu == 0x40800002 || menu == 0x40800003 || menu == 0x40800004 || menu == 0x40800005 || menu == 0x4008000F || menu == 0x40080010 || menu == 0x40080011) {
            _logger.Debug($"Skipping menu: type={menuTypeString}, index={index} {(unknown1 == 0x40 ? "" : $", Unknown1={unknown1}")} {(unknown2 == 0x00 ? "" : $", Unknown2={unknown2}")}");
            //FhUtil.set_at(0x00efbbf0, 0x40080000);
            //FhUtil.set_at(0x00efbbf4, 0xffffffff);
            atelStack->pop_int();
            FhUtil.set_at(0x01efb4d4, 0); // Don't wait for menu
            return;
        }

        if (menuType == 0x80) {
            _logger.Info($"Unknown tutorial?");
        }

        _logger.Info($"Opening menu: type={menuTypeString}, index={index} {(unknown1 == 0x40 ? "" : $", Unknown1={unknown1}")} {(unknown2 == 0x00 ? "" : $", Unknown2={unknown2}")}");
        h_SgEvent_showModularMenuInit.chain_from(SgEvent_showModularMenuInit).fnptr!(work, storage, atelStack);
    }


    public int Common_addPartyMember(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        int character = atelStack->values.as_int()[0];

        //if (!party_overridden && !(save_data->atel_is_push_member == 1)) {
        if (!party_overridden) {
            if (!is_character_unlocked(character)) {
                atelStack->pop_int();
                return 1;
            }
        }
        //logger.Debug($"add_party_member: character={id_to_character[character]}");

        return h_Common_addPartyMember.chain_from(Common_addPartyMember).fnptr!(work, storage, atelStack);
    }

    public int Common_removePartyMember(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        int character = atelStack->values.as_int()[0];

        //if (!party_overridden && !(save_data->atel_is_push_member == 1)) {
        if (!party_overridden) {
            if (is_character_unlocked(character)) {
                atelStack->pop_int();
                return 1;
            }
        }
        //logger.Debug($"remove_party_member: character={id_to_character[character]}");

        return h_Common_removePartyMember.chain_from(Common_removePartyMember).fnptr!(work, storage, atelStack);
    }

    public int Common_removePartyMemberLongTerm(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        int character = atelStack->values.as_int()[0];

        //if (!party_overridden && !(save_data->atel_is_push_member == 1)) {
        if (!party_overridden) {
            if (is_character_unlocked(character)) {
                atelStack->pop_int();
                return 1;
            }
        }
        //logger.Debug($"remove_party_member_long_term: character={id_to_character[character]}");

        return h_Common_removePartyMemberLongTerm.chain_from(Common_removePartyMemberLongTerm).fnptr!(work, storage, atelStack);
    }

    public int Common_setWeaponInvisible(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        int character = (byte)atelStack->values.as_int()[0];
        int state = atelStack->values.as_int()[1];
        _logger.Debug($"character={id_to_character[character]}, state={state}");
        if (state != 0 && is_character_unlocked(character)) {
            atelStack->pop_int();
            atelStack->pop_int();
            return 0;
        }
        return h_Common_setWeaponInvisible.chain_from(Common_setWeaponInvisible).fnptr!(work, storage, atelStack);
    }

    public int Common_putPartyMemberInSlot(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        int slot = atelStack->values.as_int()[0];
        int character = (byte)atelStack->values.as_int()[1];
        //if (!party_overridden && !(save_data->atel_is_push_member == 1)) {
        if (!party_overridden) {
            if (character == 0xff || (!is_character_unlocked(character))) {
                // Ignore call
                atelStack->pop_int();
                atelStack->pop_int();
                return (int)get_party_frontline()[slot];
            }
        }
        //logger.Debug($"put_party_member_in_slot: slot={slot}, character={(character != 0xff ? id_to_character[character] : "Empty")}");

        return h_Common_putPartyMemberInSlot.chain_from(Common_putPartyMemberInSlot).fnptr!(work, storage, atelStack);
    }

    public int Common_pushParty(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        //logger.Debug($"push_party");

        //if (party_overridden) return FhXCall.Common_pushParty.chain_from(h_Common_pushParty).fnptr!(work, storage, atelStack);
        //save_party();
        return 0;
    }

    public int Common_popParty(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        //logger.Debug($"pop_party");

        //if (party_overridden) return FhXCall.Common_popParty.chain_from(h_Common_popParty).fnptr!(work, storage, atelStack);
        //reset_party();
        return 1;
    }

    // TODO: Get function pointer instead of hook for most of these
    public void h_MsGetSavePartyMember(uint* param_1, uint* param_2, uint* param_3) {
        //logger.Debug($"get_current_party_slots");
        FhXCall.MsGetSavePartyMember.chain_from(h_MsGetSavePartyMember).fnptr!(param_1, param_2, param_3);

        //logger.Debug($"get_current_party_slots: slot_0={*param_1}, slot_1={*param_2}, slot_2={*param_3}");
    }

    // Pre-battle
    public void MsBattleExe(uint param_1, int field_idx, int group_idx, int formation_idx) {
        // Evrae is 52, 0, 0
        // Penance is 52, 1, 0
        if (field_idx == 52 && group_idx == 1 && formation_idx == 0) {
            // Try to avoid getting Penance'd by setting the group to Evrae manually
            group_idx = 0;
            _logger.Info("Tried to avoid getting Penance'd!");
        }

        var field_ptr = Battle.btl->ptr_btl_bin_fields + field_idx * 0xe;
        string field_name = Marshal.PtrToStringAnsi((nint)(field_ptr+6))!;

        var group_ptr = FhXCall.MsBtlListGroup.fnptr!(field_idx, group_idx);
        byte group_name = *(byte*)(group_ptr+5 + formation_idx * 2);

        string encounter_name = $"{field_name}_{group_name:00}";
        _logger.Debug($"{encounter_name}: param_1={param_1}, ({field_idx}, {group_idx}, {formation_idx})");
        /*
        if (encounterToPartyDict.TryGetValue(encounter_name, out List<PlySaveId> characters)) {
            if (characters.Count > 0) {
                save_party();
                set_party(characters);
            }
            else {
                reset_party();
            }
        }
         */
        if (encounterToActionDict.TryGetValue(encounter_name, out Action<ArchipelagoClientModule, ArchipelagoFFXModule>? action)) {
            action(_client!, this);
        } else {
            save_party(); // Probably?
            reset_party();
        }
        FhXCall.MsBattleExe.chain_from(MsBattleExe).fnptr!(param_1, field_idx, group_idx, formation_idx);
    }

    // Battle loop?
    public void FUN_00791820() {
        FhXCall.FUN_00791820.chain_from(FUN_00791820).fnptr!();
        string encounter_name = Marshal.PtrToStringAnsi((nint)(&Battle.btl->field_name))!;
        byte battle_end_type = Battle.btl->battle_end_type;
        byte battle_state = Battle.btl->battle_state;

        if (battle_end_type > 1 && battle_state == 0x21) {
            if (party_overridden) {
                reset_party();
                // Battle frontline gets copied after this so have to set here
                for (int i = 0; i < 3; i++) {
                    Battle.btl->__0x1FC5[i] = save_data->party_frontline[i];
                }
                for (int i = 0; i < 4; i++) {
                    Battle.btl->__0x1FD3[i] = save_data->party_backline[i];
                }
                party_overridden = false;
            }
            switch (battle_end_type) {
                case 2: // Battle Victory
                    _logger.Info($"Victory: type={battle_end_type}, encounter={encounter_name}");
                    if (encounterVictoryActions.TryGetValue(encounter_name!, out Action<ArchipelagoClientModule, ArchipelagoFFXModule>? victoryAction)) {
                        victoryAction(_client!, this);
                    }
                    if (encounterToLocationDict.TryGetValue(encounter_name!, out int[]? boss_locations)) {
                        foreach (int location_id in boss_locations) {
                            // Sending all locations even if they don't exist
                            if (_client!.sendLocation(location_id, ArchipelagoClientModule.ArchipelagoLocationType.Boss) && item_locations.boss.TryGetValue(location_id, out var item)) {
                                obtain_item(item.id);
                            }
                        }
                    }
                    break;
                case 3: // Battle Escape
                    _logger.Info($"Escape: type={battle_end_type}, encounter={encounter_name}");
                    if (encounterEscapeActions.TryGetValue(encounter_name!, out Action<ArchipelagoClientModule, ArchipelagoFFXModule>? escapeAction)) {
                        escapeAction(_client!, this);
                    }
                    break;
                default:
                    _logger.Info($"Battle End: type={battle_end_type}, encounter={encounter_name}");
                    break;
            }
        }
    }

    private void* curr_pos_area_ptr = null;
    public byte MsBtlReadSetScene() {
        byte result = FhXCall.MsBtlReadSetScene.chain_from(MsBtlReadSetScene).fnptr!();
        _logger.Info($"Battle Name: {Marshal.PtrToStringAnsi((nint)FhUtil.ptr_at<char>(0xD2C25A))}");
        //ref BtlAreas original_pos_struct = ref *Battle.btl->ptr_pos_def;
        BtlAreasHelper original_pos_struct = new BtlAreasHelper(Battle.btl->ptr_pos_def);
        if (original_pos_struct.areas.Length == 0) return result;
        if (curr_pos_area_ptr != null) {
            NativeMemory.Free(curr_pos_area_ptr);
            curr_pos_area_ptr = null;
            //Marshal.FreeHGlobal((nint)curr_pos_area);
        }
        int total_size = 0;
        int total_info_count = 0;
        for (int i = 0; i < original_pos_struct.areas.Length; i++) {
            total_size += 0x60;
            total_size += Math.Max(original_pos_struct.areas[i].party_pos.Length, (byte)3) * 0x20;
            total_size += Math.Max(original_pos_struct.areas[i].aeon_pos.Length, (byte)0xC) * 0x20;
            total_size += original_pos_struct.areas[i].enemy_pos.Length * 0x20;
            total_size += original_pos_struct.areas[i].some_info.Length * 0x10; // some_info
            total_info_count += original_pos_struct.areas[i].some_info.Length;
            for (int j = 0; j < original_pos_struct.areas[i].some_info.Length; j++) {
                total_size += original_pos_struct.areas[i].something[j].Length * 0x10;
            }
        }
        total_size += 0x10; //chunk_end
        curr_pos_area_ptr = NativeMemory.AllocZeroed((uint)total_size);
        //curr_pos_area = (PosArea*)Marshal.AllocHGlobal(total_size);

        BtlArea* curr_pos_struct_ptr = (BtlArea*)curr_pos_area_ptr;
        BtlAreasHelper curr_pos_struct = new BtlAreasHelper(curr_pos_struct_ptr);
        //
        //ref BtlAreas curr_pos_struct = ref *curr_pos_struct_ptr;
        curr_pos_struct.area_count = original_pos_struct.area_count;
        curr_pos_struct.area_type = original_pos_struct.area_type;

        // Copy
        int some_info_start = original_pos_struct.areas.Length * 0x60;
        int curr_info_count = 0;
        int positions_start = some_info_start + total_info_count * 0x10;
        int curr_position_count = 0;
        for (int i = 0; i < original_pos_struct.areas.Length; i++) {
            //curr_pos_struct.areas[i] = original_pos_struct.areas[i];
            curr_pos_struct.areas[i].btlArea = original_pos_struct.areas[i].btlArea;

            curr_pos_struct.areas[i].btlArea.count_party_pos = Math.Max(curr_pos_struct.areas[i].btlArea.count_party_pos, (byte)3);
            curr_pos_struct.areas[i].btlArea.count_aeon_pos = Math.Max(curr_pos_struct.areas[i].btlArea.count_aeon_pos, (byte)0xc);
            curr_pos_struct.areas[i].btlArea.count_enemy_pos = curr_pos_struct.areas[i].btlArea.count_enemy_pos;

            curr_pos_struct.areas[i].btlArea.offset_party_pos = (uint)(positions_start + curr_position_count * 0x10);
            for (int j = 0; j < curr_pos_struct.areas[i].party_pos.Length; j++) {

                if (original_pos_struct.areas[i].party_pos.Length <= 2 || (original_pos_struct.areas[i].party_pos[0] - original_pos_struct.areas[i].party_pos[original_pos_struct.areas[i].party_pos.Length - 1]).Length() < 10) {
                    // TODO: Calculate positions
                    if (original_pos_struct.areas[i].party_pos.Length == 1 || original_pos_struct.areas[i].party_pos.Length >= 3) {
                        if (j > 2) {
                            //logger.Warning($"Unexpected position: {param_1}, {btl_pos_a}, {btl_pos_b}, {btl_pos_c}");
                        }
                        curr_pos_struct.areas[i].party_pos[j] = original_pos_struct.areas[i].party_pos[0];
                        if (j != 1) {
                            Vector4 enemy_pos = original_pos_struct.areas[i].enemy_pos[0];
                            Vector4 enemy_to_party = curr_pos_struct.areas[i].party_pos[j] - enemy_pos;
                            Vector3 enemy_to_party_3d = new(enemy_to_party.X, enemy_to_party.Y, enemy_to_party.Z);
                            Vector3 rotation_axis = new(0, 1, 0);
                            double angle = j == 0 ? -(Math.PI / 2) : (Math.PI / 2);

                            Vector3 x = Vector3.Cross(rotation_axis, enemy_to_party_3d);
                            Vector3 y = Vector3.Cross(rotation_axis, x);
                            Vector3 rotated = enemy_to_party_3d + (float)Math.Sin(angle) * x + (float)(1 - Math.Cos(angle)) * y;
                            curr_pos_struct.areas[i].party_pos[j] += new Vector4(Vector3.Normalize(rotated) * 30, curr_pos_struct.areas[i].party_pos[j].W);
                        }
                    }
                    else {
                        //if (j > 2) return -1;
                        Vector4 ally_1 = original_pos_struct.areas[i].party_pos[0];
                        Vector4 line = curr_pos_struct.areas[i].party_pos[1] - ally_1;
                        if (line.Length() > 10) {
                            if (j == 2) {
                                curr_pos_struct.areas[i].party_pos[j] = original_pos_struct.areas[i].party_pos[1];
                                curr_pos_struct.areas[i].party_pos[j] += line;
                            }
                            else {
                                curr_pos_struct.areas[i].party_pos[j] = original_pos_struct.areas[i].party_pos[j];
                            }
                        }
                        else if (j != 1) {
                            Vector4 enemy_pos = original_pos_struct.areas[i].enemy_pos[0];
                            Vector4 enemy_to_party = curr_pos_struct.areas[i].party_pos[j] - enemy_pos;
                            Vector3 enemy_to_party_3d = new(enemy_to_party.X, enemy_to_party.Y, enemy_to_party.Z);
                            Vector3 rotation_axis = new(0, 1, 0);
                            double angle = j == 0 ? -(Math.PI / 2) : (Math.PI / 2);

                            Vector3 x = Vector3.Cross(rotation_axis, enemy_to_party_3d);
                            Vector3 y = Vector3.Cross(rotation_axis, x);
                            Vector3 rotated = enemy_to_party_3d + (float)Math.Sin(angle) * x + (float)(1 - Math.Cos(angle)) * y;
                            curr_pos_struct.areas[i].party_pos[j] += new Vector4(Vector3.Normalize(rotated) * 30, curr_pos_struct.areas[i].party_pos[j].W);
                        }
                    }
                }
                else {
                    // Copy
                    curr_pos_struct.areas[i].party_pos[j] = original_pos_struct.areas[i].party_pos[j];
                }
                curr_position_count++;
            }
            curr_pos_struct.areas[i].btlArea.offset_party_run_pos = (uint)(positions_start + curr_position_count * 0x10);
            for (int j = 0; j < curr_pos_struct.areas[i].party_pos.Length; j++) {
                if (original_pos_struct.areas[i].party_pos.Length <= j) {
                    curr_pos_struct.areas[i].party_run_pos[j] = original_pos_struct.areas[i].party_run_pos[0];
                }
                else {
                    curr_pos_struct.areas[i].party_run_pos[j] = original_pos_struct.areas[i].party_run_pos[j];
                }
                if (curr_pos_struct.areas[i].party_run_pos[j] == Vector4.Zero) {
                    curr_pos_struct.areas[i].party_run_pos[j] = original_pos_struct.areas[i].party_pos[0];
                    Vector4 enemy_pos = original_pos_struct.areas[i].enemy_pos[0];
                    Vector4 enemy_to_party = curr_pos_struct.areas[i].party_run_pos[j] - enemy_pos;
                    curr_pos_struct.areas[i].party_run_pos[j] += Vector4.Normalize(enemy_to_party) * 0x30;
                }
                curr_position_count++;
            }

            curr_pos_struct.areas[i].btlArea.offset_aeon_pos = (uint)(positions_start + curr_position_count * 0x10);
            for (int j = 0; j < curr_pos_struct.areas[i].aeon_pos.Length; j++) {
                if (j == 7) {
                    curr_pos_struct.areas[i].aeon_pos[j] = curr_pos_struct.areas[i].party_pos[0];
                }
                else if (j == 9) {
                    curr_pos_struct.areas[i].aeon_pos[j] = curr_pos_struct.areas[i].party_pos[2];
                }
                else {
                    curr_pos_struct.areas[i].aeon_pos[j] = curr_pos_struct.areas[i].party_pos[1];
                }
                curr_position_count++;
            }
            curr_pos_struct.areas[i].btlArea.offset_aeon_run_pos = (uint)(positions_start + curr_position_count * 0x10);
            for (int j = 0; j < curr_pos_struct.areas[i].aeon_pos.Length; j++) {
                curr_pos_struct.areas[i].aeon_run_pos[j] = curr_pos_struct.areas[i].party_run_pos[0];
                curr_position_count++;
            }

            curr_pos_struct.areas[i].btlArea.offset_enemy_pos = (uint)(positions_start + curr_position_count * 0x10);
            for (int j = 0; j < curr_pos_struct.areas[i].enemy_pos.Length; j++) {
                curr_pos_struct.areas[i].enemy_pos[j] = original_pos_struct.areas[i].enemy_pos[j];
                curr_position_count++;
            }
            curr_pos_struct.areas[i].btlArea.offset_enemy_run_pos = (uint)(positions_start + curr_position_count * 0x10);
            for (int j = 0; j < curr_pos_struct.areas[i].enemy_pos.Length; j++) {
                curr_pos_struct.areas[i].enemy_run_pos[j] = original_pos_struct.areas[i].enemy_run_pos[j];
                curr_position_count++;
            }

            curr_pos_struct.areas[i].btlArea.offset_some_info = (uint)(some_info_start + curr_info_count * 0x10);
            for (int j = 0; j < curr_pos_struct.areas[i].some_info.Length; j++) {
                curr_pos_struct.areas[i].some_info[j] = original_pos_struct.areas[i].some_info[j];
                curr_info_count++;
            }
        }

        for (int i = 0; i < original_pos_struct.areas.Length; i++) {
            for (int j = 0; j < curr_pos_struct.areas[i].some_info.Length; j++) {
                curr_pos_struct.areas[i].some_info[j].offset_something = (uint)(positions_start + curr_position_count * 0x10);
                for (int k = 0; k < curr_pos_struct.areas[i].something[j].Length; k++) {
                    curr_pos_struct.areas[i].something[j][k] = original_pos_struct.areas[i].something[j][k];
                    curr_position_count++;
                }
            }
        }
        for (int i = 0; i < original_pos_struct.areas.Length; i++) {
            curr_pos_struct.areas[i].btlArea.offset_chunk_end = (uint)(positions_start + curr_position_count * 0x10);
        }
        *curr_pos_struct.chunk_end = *original_pos_struct.chunk_end;


        Battle.btl->ptr_pos_def = curr_pos_struct_ptr;

        return result;
    }

    public int h_MsBtlGetPos(int param_1, Chr* chr, int btl_pos_a, int btl_pos_b, int btl_pos_c, Vector4* out_pos) {
        // btl_pos_a: position group/set/area
        // btl_pos_b = 3: party_pos[btl_pos_c]
        // btl_pos_b = 4: aeon_pos[btl_pos_c]
        // btl_pos_b = 5: enemy_pos[btl_pos_c]
        // btl_pos_b = 6: party_run_pos[btl_pos_c]?
        // btl_pos_b = 7: aeon_run_pos[btl_pos_c]?
        // btl_pos_b = 8: enemy_run_pos[btl_pos_c]?
        // btl_pos_b = 9: b?
        // btl_pos_b = 10: c?
        return FhXCall.MsBtlGetPos.chain_from(h_MsBtlGetPos).fnptr!(param_1, chr, btl_pos_a, btl_pos_b, btl_pos_c, out_pos);
    }

    private int give_item(uint item_id, int amount) {
        _logger.Debug($"give_item: {amount} x {item_id}");

        int new_amount = (int)(save_data->get_item_count((int)item_id) + amount);
        if (new_amount > 99) {
            int excess = new_amount - 99;
            int old_excess = excess_inventory.GetValueOrDefault(item_id, 0);
            excess_inventory[item_id] = excess + old_excess;
            amount -= excess;
        }

        return FhXCall.MsSaveItemUse.chain_from(give_item).fnptr!(item_id, amount);
    }

    private void h_TkMsImportantSet(uint param_1) {
        _logger.Debug($"give_key_item: {param_1}");

        FhXCall.TkMsImportantSet.chain_from(h_TkMsImportantSet).fnptr!(param_1);
    }

    private nint h_read_from_bin(int param_1, nint param_2, int* param_3) {
        _logger.Debug($"get_from_bin: {param_1}, {(int)param_2}, {*param_3}");
        _logger.Debug($"bin_pointers: {(uint)param_2}, {(uint)(*takara_pointer)}, {(uint)(*buki_get_pointer)}");
        if (param_2 == *takara_pointer) {
            _logger.Debug("get_from_bin: takara.bin");
        }
        else if (param_2 == *buki_get_pointer) {
            _logger.Debug("get_from_bin: buki_get.bin");
        }
        var result = FhXCall.MsGetExcelData.chain_from(h_read_from_bin).fnptr!(param_1, param_2, param_3);
        _logger.Debug($"get_from_bin: result = {((uint)result).ToString("X")}");
        return result;
    }

    private ushort h_get_weapon_name(Equipment* param_1) {
        _logger.Debug($"get_weapon_name: {(int)param_1}");

        return FhXCall.MsWeaponNameNum.chain_from(h_get_weapon_name).fnptr!(param_1);
    }

    private void h_get_weapon_model(ushort name_id, byte owner, int unknown, ushort* model_id_pointer) {
        _logger.Debug($"get_weapon_model: {name_id}, {owner}, {unknown}, {*model_id_pointer}, ");

        FhXCall.MsWeaponName.chain_from(h_get_weapon_model).fnptr!(name_id, owner, unknown, model_id_pointer);
    }

    private void h_obtain_treasure_cleanup(BtlRewardData* param_1, int param_2) {
        _logger.Debug("obtain_treasure_cleanup");

        FhXCall.FUN_007993f0.chain_from(h_obtain_treasure_cleanup).fnptr!(param_1, param_2);
    }

    public void obtain_item(uint item_id, int amount=-1) {
        if (item_id == 0) return;
        var item_type = (item_id & 0xF000) >> 12;
        if (amount == -1) amount = (int)((item_id & 0xFF0000) >> 16);
        item_id &= 0xFFFF;
        int count;
        switch (item_type) {
            case 0xA:
                // Key Item

                // Progressive Mirror
                if (item_id == 0xA002 && save_data->key_items.get((int)item_id)) {
                    //Globals.save_data->key_items.set((int)item_id, false);
                    item_id = 0xA003;
                }

                // Al Bhed Primers
                if (item_id == 0xA004 && save_data->key_items.get((int)item_id)) {
                    for (byte i = 0; i < 26; i++)
                    {
                        if (!save_data->unlocked_primers.get_bit(i))
                        {
                            item_id += i;
                            break;
                        }
                    }
                }

                // Progressive Jecht's Sphere
                if (item_id == 0xA020) {
                    save_data->jecht_spheres.collected_amount++;

                    if (save_data->jecht_spheres.collected_amount >= 1)
                        _overdrives!.send_overdrive(PlayerCommandId.PCOM_SHOOTING_STAR);

                    if (save_data->jecht_spheres.collected_amount >= 3)
                        _overdrives!.send_overdrive(PlayerCommandId.PCOM_BANISHING_BLADE);

                    if (save_data->jecht_spheres.collected_amount >= 10)
                        _overdrives!.send_overdrive(PlayerCommandId.PCOM_TORNADO);
                }

                h_TkMsImportantSet(item_id);
                break;
            case 0x2:
                // Item
                give_item(item_id, amount);
                break;
            case 0x5:
                // Equipment
                item_id &= 0xFFF;

                //UnownedEquipment* weapon_data = (UnownedEquipment*)h_read_from_bin((int)item_id, (short*)(*buki_get_pointer), 0);
                if (!seed.Gear.TryGetValue((int)item_id, out ArchipelagoGear? gear)) {
                    _logger.Error($"Gear {item_id} doesn't exist");
                    break;
                }

                UnownedEquipment weapon_data = new(){
                    flags = gear.flags,
                    owner = gear.owner,
                    type = gear.type,
                    dmg_formula = gear.dmg_formula,
                    power = gear.power,
                    crit_bonus = gear.crit_bonus,
                    slot_count = gear.slot_count,
                };
                for (int i = 0; i < 4; i++) {
                    weapon_data.abilities[i] = i < gear.abilities.Length ? (ushort)(gear.abilities[i] | 0x8000) : (ushort)0xFF;
                }

                //var data = get_from_bin((int)item_id, (short*)0x12000C00, 0);
                if (weapon_data.is_celestial) {
                    foreach (var equip in save_data->equipment) {
                        if (equip.exists && equip.owner == weapon_data.owner && equip.is_celestial) {
                            // Upgrade celestial
                            if (celestial_level[equip.owner] < 2) {
                                celestial_level[equip.owner] += 1;
                                FhXCall.TkSetLegendAbility.chain_from(TkSetLegendAbility).fnptr!(equip.owner, celestial_level[equip.owner]);
                            }
                            return;
                        }
                    }
                } else if (weapon_data.is_brotherhood) {
                    foreach (ref var equip in save_data->equipment) {
                        if (equip.exists && equip.is_brotherhood) {
                            if (equip.is_hidden) {
                                // Obtain
                                equip.is_hidden = false;
                            } else {
                                // Upgrade
                                equip.flags = weapon_data.flags;
                                equip.owner = weapon_data.owner;
                                equip.type = weapon_data.type;
                                equip.dmg_formula = weapon_data.dmg_formula;
                                equip.power = weapon_data.power;
                                equip.crit_bonus = weapon_data.crit_bonus;
                                equip.slot_count = weapon_data.slot_count;
                                for (int i = 0; i < 4; i++) {
                                    equip.abilities[i] = weapon_data.abilities[i];
                                }
                            }
                            break;
                        }
                    }
                    return;
                }


                BtlRewardData rewardData = new BtlRewardData();
                rewardData.gear_count = 1;
                Equipment new_weapon = rewardData.gear[0];
                new_weapon.exists = true;
                new_weapon.flags = weapon_data.flags;
                new_weapon.owner = weapon_data.owner;
                new_weapon.type = weapon_data.type;
                new_weapon.equipped_by = 0xff;
                new_weapon.dmg_formula = weapon_data.dmg_formula;
                new_weapon.power = weapon_data.power;
                new_weapon.crit_bonus = weapon_data.crit_bonus;
                count = 0;
                for (int i = 0; i < 4; i++) {
                    if (weapon_data.abilities[i] is 0x00 or 0xFF) {
                        new_weapon.abilities[i] = 0xff;
                    } else {
                        new_weapon.abilities[i] = weapon_data.abilities[i];
                        count++;
                    }
                }
                new_weapon.slot_count = (byte)Math.Max(weapon_data.slot_count, count);
                new_weapon.name_id = h_get_weapon_name(&new_weapon);
                h_get_weapon_model(new_weapon.name_id, new_weapon.owner, 0, &new_weapon.model_id);
                var result = FhXCall.FUN_007ab930.fnptr!(&new_weapon); // giveWeapon?
                if (result != 0) {
                    h_obtain_treasure_cleanup(&rewardData, 7);
                }
                break;
            case 0x1:
                // Gil
                if (amount == -1) amount = (int)((item_id & 0xFF0000) >> 16) * 1000;
                FhXCall.MsPayGIL.fnptr!(-amount);
                break;
            case 0xE:
                // Region Unlock
                item_id &= 0xff;
                _logger.Debug($"Region: {(RegionEnum)item_id}");
                region_is_unlocked[(RegionEnum)item_id] = true;
                break;
            case 0xF:
                // Party Member
                int char_id = (int)(item_id & 0xFF);
                _logger.Debug($"Character: {id_to_character[char_id]}");
                unlocked_characters[char_id] = true;
                if (char_id == 0xF) {
                    // Magus sisters
                    unlocked_characters[PlySaveId.PC_MAGUS2] = true;
                    unlocked_characters[PlySaveId.PC_MAGUS3] = true;
                }
                save_party();
                reset_party();
                if (seed.Options.GoalRequirement == GoalRequirement.PartyMembers || seed.Options.GoalRequirement == GoalRequirement.PartyMembersAndAeons) {
                    int num_unlocked;
                    int num_required;
                    if (seed.Options.GoalRequirement == GoalRequirement.PartyMembers) {
                        num_unlocked = unlocked_characters.Where(x => x.Key < 8 && x.Value).Count();
                        num_required = Math.Min(seed.Options.RequiredPartyMembers, 8);
                    } else {
                        num_unlocked = unlocked_characters.Where(x => x.Key < 16 && x.Value).Count();
                        num_required = seed.Options.RequiredPartyMembers;
                    }
                    string message = $"{num_unlocked}/{num_required} party members unlocked";
                    Color color = Color.White;
                    if (unlocked_characters.Count(x => x.Value) >= seed.Options.RequiredPartyMembers) {
                        color = Color.Green;
                    }
                    _gui!.add_log_message([(message, color)]);
                }
                break;
            case 0x9:
                // Trap
                item_id &= 0xfff;
                _logger.Debug($"Trap: {item_id}");
                if (item_id == 0) {
                    queued_voice_lines.Enqueue(voicelines[rng.Next(voicelines.Length)]);
                }
                break;
            case 0x3:
                // Overdrive
                _logger.Debug($"Overdrive: {item_id}");
                other_inventory.TryGetValue(item_id, out count);
                other_inventory[item_id] = count + 1;

                switch (item_id) {
                    case >= PlayerCommandId.PCOM_SPIRAL_CUT and <= PlayerCommandId.PCOM_BLITZ_ACE:
                        OverdriveModule.OverdriveProvider.provide_overdrive(PlySaveId.PC_TIDUS);
                        break;
                    case >= PlayerCommandId.PCOM_SHOOTING_STAR and <= PlayerCommandId.PCOM_TORNADO:
                        OverdriveModule.OverdriveProvider.provide_overdrive(PlySaveId.PC_AURON);
                        break;
                    case >= PlayerCommandId.PCOM_JUMP and <= PlayerCommandId.PCOM_NOVA:
                        OverdriveModule.OverdriveProvider.provide_overdrive(PlySaveId.PC_KIMAHRI);
                        break;
                    case >= PlayerCommandId.PCOM_ELEMENT_REELS and <= PlayerCommandId.PCOM_AUROCHS_REELS:
                        OverdriveModule.OverdriveProvider.provide_overdrive(PlySaveId.PC_WAKKA);
                        break;
                    case PlayerCommandId.PCOM_REQUIEM:
                        OverdriveModule.OverdriveProvider.provide_overdrive(PlySaveId.PC_SEYMOUR);
                        break;
                    case PlayerCommandId.PCOM_ENERGY_BLAST:
                        OverdriveModule.OverdriveProvider.provide_overdrive(PlySaveId.PC_VALEFOR);
                        break;
                    default:
                        throw new NotImplementedException();
                }
                break;
            case 0xC:
                // Other
                _logger.Debug($"Other: {item_id}");
                other_inventory.TryGetValue(item_id, out count);
                other_inventory[item_id] = count+1;
                break;
        }
    }

    private Random rng = new Random();

    public void call_obtain_brotherhood() {
        AtelStack stack = new AtelStack();

        int param_1 = 0;
        int param_2 = 0;

        h_Common_obtainBrotherhoodRetInt.fnptr!((AtelBasicWorker*)&param_1, &param_2, &stack);
    }

    public void receive_treasure(int id) {
        AtelStack stack = new AtelStack();
        stack.push_int(id);

        int param_1 = 0;
        int param_2 = 0;

        h_Common_obtainTreasureSilentlyInit.chain_from(Common_obtainTreasureSilentlyInit).fnptr!((AtelBasicWorker*)&param_1, &param_2, &stack);
    }

    public void set_airship_destinations() {
        _logger.Debug("set_airship_destinations");
        AtelBasicWorker* worker = Atel.controllers[0].worker(0);
        var var_table = worker->table_event_data;
        uint* airshipDestinationCount = &var_table[11];
        uint* airshipDestinationLength = &var_table[12];
        uint* airshipDestinations = &var_table[13];

        for (int i = 0; i < 31; i++) {
            airshipDestinations[i] = 0;
        }
        uint curr = 0;
        foreach (var region in region_is_unlocked) {
            if (region.Value) {
                var state = region_states[region.Key];
                if (state.airship_destination_index != 99) {
                    airshipDestinations[curr] = state.airship_destination_index;
                    curr++;
                }
            }
        }
        *airshipDestinationCount = curr - 1;
        *airshipDestinationLength = curr - 1;
    }


    // Sphere Grid Experiment

    public void eiAbmParaGet() {
        // TODO: Replace normal calculation with custom Archipelago-based calculation (if option enabled)
        _logger.Debug("Calculating stats");
        FhXCall.eiAbmParaGet.chain_from(eiAbmParaGet).fnptr!();

        //foreach (ref PlySave ply in save_data->ply_saves) {
        //    ply.abi_map.has_extract_power   = true;
        //    ply.abi_map.has_extract_mana    = true;
        //    ply.abi_map.has_extract_speed   = true;
        //    ply.abi_map.has_extract_ability = true;
        //}
    }

    private void MsSetRamChrParam(uint chr_id) {
        _logger.Debug($"MsSetRamChrParam: {chr_id}");
        FhXCall.MsSetRamChrParam.chain_from(MsSetRamChrParam).fnptr!(chr_id);

        Chr* chr = FhXCall.MsGetChr.fnptr!((int)chr_id);

        if (seed.Options.AlwaysSensor == 1) {
            chr->ram.auto_ability_effects.has_sensor = true;
        }
    }

    private void MsSetSaveParam(uint chr_id) {
        _logger.Debug("Calculating base stats and equipment");
        FhXCall.MsSetSaveParam.chain_from(MsSetSaveParam).fnptr!(chr_id);

        // Does nothing??
        if (seed.Options.AlwaysSensor == 1) {
            save_data->ply_saves[(int)chr_id].auto_ability_effects.has_sensor = true;
        }

        return;
        //PlySave ply = save_data->ply_saves[(int)chr_id];
        //Equipment*[] equips =
        //[
        //    (Equipment*)FhXCall.MsGetSaveWeapon.fnptr!(ply.wpn_inv_idx, 0x0),
        //    (Equipment*)FhXCall.MsGetSaveWeapon.fnptr!(ply.arm_inv_idx, 0x0),
        //];
        //int strength_mult = 0;
        //int defense_mult = 0;
        //int magic_mult = 0;
        //int magic_defense_mult = 0;
        //int agility_mult = 0;
        //int luck_mult = 0;
        //int evasion_mult = 0;
        //int accuracy_mult = 0;
        //int hp_mult = 0;
        //int mp_mult = 0;
        //int strength_bonus_mult = 0;
        //int defense_bonus_mult = 0;
        //int magic_bonus_mult = 0;
        //int magic_defense_bonus_mult = 0;

        //foreach (Equipment* equip in equips) {
        //    for (int i = 0; i < 4; i++) {
        //        if (equip->abilities[i] == 0 || equip->abilities[i] == 0xFF) continue;
        //        AutoAbility* a_ability = (AutoAbility*)FhXCall.MsGetExcelData.fnptr!(equip->abilities[i] & 0xFFF, Battle.btl->ptr_a_ability_bin, (int*)0x0);
        //        strength_mult += a_ability->stat_inc_flags.strength() ? a_ability->stat_inc_amount : 0;
        //        defense_mult += a_ability->stat_inc_flags.defense() ? a_ability->stat_inc_amount : 0;
        //        magic_mult += a_ability->stat_inc_flags.magic() ? a_ability->stat_inc_amount : 0;
        //        magic_defense_mult += a_ability->stat_inc_flags.magic_defense() ? a_ability->stat_inc_amount : 0;
        //        agility_mult += a_ability->stat_inc_flags.agility() ? a_ability->stat_inc_amount : 0;
        //        luck_mult += a_ability->stat_inc_flags.luck() ? a_ability->stat_inc_amount : 0;
        //        evasion_mult += a_ability->stat_inc_flags.evasion() ? a_ability->stat_inc_amount : 0;
        //        accuracy_mult += a_ability->stat_inc_flags.accuracy() ? a_ability->stat_inc_amount : 0;
        //        hp_mult += a_ability->stat_inc_flags.hp() ? a_ability->stat_inc_amount : 0;
        //        mp_mult += a_ability->stat_inc_flags.mp() ? a_ability->stat_inc_amount : 0;
        //        strength_bonus_mult += a_ability->stat_inc_flags.strength_bonus() ? a_ability->stat_inc_amount : 0;
        //        defense_bonus_mult += a_ability->stat_inc_flags.defense_bonus() ? a_ability->stat_inc_amount : 0;
        //        magic_bonus_mult += a_ability->stat_inc_flags.magic_bonus() ? a_ability->stat_inc_amount : 0;
        //        magic_defense_bonus_mult += a_ability->stat_inc_flags.magic_defense_bonus() ? a_ability->stat_inc_amount : 0;
        //    }
        //}

        //ply.strength = (byte)Math.Clamp(ply.strength * strength_mult / 100, 0, 255);
        //ply.defense = (byte)Math.Clamp(ply.defense * defense_mult / 100, 0, 255);
        //ply.magic = (byte)Math.Clamp(ply.magic * magic_mult / 100, 0, 255);
        //ply.magic_defense = (byte)Math.Clamp(ply.magic_defense * magic_defense_mult / 100, 0, 255);
        //ply.agility = (byte)Math.Clamp(ply.agility * agility_mult / 100, 0, 255);
        //ply.luck = (byte)Math.Clamp(ply.luck * luck_mult / 100, 0, 255);
        //ply.evasion = (byte)Math.Clamp(ply.evasion * evasion_mult / 100, 0, 255);
        //ply.accuracy = (byte)Math.Clamp(ply.accuracy * accuracy_mult / 100, 0, 255);
        //ply.hp = (uint)Math.Clamp(ply.hp * hp_mult / 100, 0, ply.auto_ability_effects.has_break_hp_limit ? 99999 : 9999);
        //ply.mp = (uint)Math.Clamp(ply.mp * mp_mult / 100, 0, ply.auto_ability_effects.has_break_mp_limit ? 9999 : 999);
    }

    public void h_FUN_00a48910(uint chr_id, int node_idx) {
        // TODO: Send Archipelago location when node is unlocked (if option enabled)
        _logger.Debug($"Unlock node {node_idx} for {id_to_character[chr_id]}");
        FhXCall.FUN_00a48910.chain_from(h_FUN_00a48910).fnptr!(chr_id, node_idx);
    }

    private uint MsApUp(int chr_id, Chr* chr, int base_ap_add, uint param_4) {
        _logger.Debug($"AP gain: character={id_to_character[chr_id]}, ap={base_ap_add}");
        int new_ap = (int)Math.Min(Math.BigMul(base_ap_add, ap_multiplier), 999_999_999);
        return FhXCall.MsApUp.chain_from(MsApUp).fnptr!(chr_id, chr, new_ap, param_4);
    }



    // Non-loading FMV
    private bool graphicInitFMVPlayer(int movie_id, int param_2) {
        bool result = FhXCall.graphicInitFMVPlayer.chain_from(graphicInitFMVPlayer).fnptr!(movie_id, param_2);
        return !result;
    }

    // Texture experiments
    private void h_FUN_00656c90(int param_1, int param_2, char* fileName) {
        //logger.Debug($"{param_1}, {param_2}, {(nint)fileName}");
        //
        //string nameString = Marshal.PtrToStringAnsi((nint)fileName);
        //
        //logger.Debug(nameString);


        FhXCall.FUN_00656c90.chain_from(h_FUN_00656c90).fnptr!(param_1, param_2, fileName);
    }

    private void TkMenuAppearMainCmdWindow(int param_1, int param_2) {
        // All menu options are enabled at progress 0
        ushort progress = save_data->story_progress;
        save_data->story_progress = 0;
        FhXCall.TkMenuAppearMainCmdWindow.chain_from(TkMenuAppearMainCmdWindow).fnptr!(param_1, param_2);
        save_data->story_progress = progress;
    }

    // Voice related
    //public nint h_FMOD_EventSystem_load(nint param_1, nint file_path, nint param_3, nint param_4) {
    //    string path = Marshal.PtrToStringAnsi(file_path);
    //    nint result = _FMOD_EventSystem_load.orig_fptr(param_1, file_path, param_3, param_4);
    //    _logger.Debug($"{path}, {param_1}, {param_3} -> {result}");

    //    return result;
    //}

    public void h_FfxFmod_soundInit_setLang(nint ffxFmod, int lang) {
        FhXCall.FfxFmod_soundInit_setLang.chain_from(h_FfxFmod_soundInit_setLang).fnptr!(ffxFmod, lang);
        *(byte*)(ffxFmod + 4) = 0;
    }

    // Unsure if there are side effects
    public void LocalizationManager_Initialize(LocalizationManager* localizationManager) {
        FhXCall.LocalizationManager_Initialize.chain_from(LocalizationManager_Initialize).fnptr!(localizationManager);
        if (TextLanguage.HasValue) {
            _logger.Debug($"Text: {TextLanguage.Value}");
            localizationManager->lang_text = TextLanguage.Value;
        }
        if (VoiceLanguage.HasValue) {
            _logger.Debug($"Voice: {VoiceLanguage.Value}");
            localizationManager->lang_video = VoiceLanguage.Value;
            localizationManager->lang_voice = VoiceLanguage.Value;
        }
    }

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public unsafe delegate nint FMOD_Bank_Post_Load(nint param_1, nint param_2, nint param_3, nint param_4);

    public int FmodVoice_dataChange(nint FmodVoice, int event_id, nint param_2) {
        _logger.Debug($"{FmodVoice}, {event_id}, {param_2}");
        int result = FhXCall.FmodVoice_dataChange.chain_from(FmodVoice_dataChange).fnptr!(FmodVoice, event_id, param_2);
        //string bank_name = "ffx_us_voice03"; // Contains "Stay away from the summoner!" (136815042)
        foreach (string bank_name in (string[])["ffx_us_voice03", "ffx_us_voice07", "ffx_us_voice11", "ffx_us_voice12", "ffx_us_voice20"]) {
            string path = $"../../../FFX_Data/GameData/PS3Data/Sound_PC/Voice/US/{bank_name}.fev";
            nint file_path = Marshal.StringToHGlobalAnsi(path);

            int bank_index = bank_name[^1] + (bank_name[^2] * 5 - 0x108)*2;
            nint bank = *(int*)(FmodVoice+0x18) + bank_index*4;

            nint load_result = FhXCall.FMOD_EventSystem_load.fnptr!(param_2, file_path, 0, bank);
            Marshal.FreeHGlobal(file_path);

            if (load_result == 0) {
                int* piVar5 = *(int**)bank;
                if (piVar5 != null) {
                    FMOD_Bank_Post_Load _FMOD_Bank_Post_Load = Marshal.GetDelegateForFunctionPointer<FMOD_Bank_Post_Load>(*(nint*)(*piVar5 + 8));

                    nint s_voice = Marshal.StringToHGlobalAnsi("voice");
                    load_result = _FMOD_Bank_Post_Load((nint)piVar5, s_voice, 0, *(int*)(FmodVoice + 0xc) + bank_index*4);
                    Marshal.FreeHGlobal(s_voice);
                    _logger.Debug($"{bank_name}: {load_result}");
                }
            }

        }

        return result;
    }

    public Queue<int> queued_voice_lines = [];
    public void play_voice_line(int voice_line) {
        AtelStack stack = new AtelStack();
        stack.push_int(voice_line);
        _logger.Debug($"stack_size = {stack.size}, voice_line = {stack.values.as_int()[0]}");

        int work = 0;
        int[] storage_array = [0,0,0,0];

        fixed (int* storage = storage_array) {
            h_Common_playFieldVoiceLineInit.fnptr!((AtelBasicWorker*)&work, storage, &stack);
            h_Common_playFieldVoiceLineExec.fnptr!((AtelBasicWorker*)&work, &stack);
            h_Common_playFieldVoiceLineResultInt.fnptr!((AtelBasicWorker*)&work, storage, &stack);

            h_Common_00D6Init.fnptr!((AtelBasicWorker*)&work, storage, &stack);
            h_Common_00D6Exec.fnptr!((AtelBasicWorker*)&work, &stack);
            h_Common_00D6ResultInt.fnptr!((AtelBasicWorker*)&work, storage, &stack);

        }

    }

    private Dictionary<uint, string> item_name_cache = [];
    public string get_item_name(uint item_id) {
        if (item_name_cache.TryGetValue(item_id, out var name)) return name;
        byte* item_name;
        FhXCall.TkMsGetRomItem.fnptr!(item_id, (int*)&item_name);
        byte[] decoded = new byte[FhEncoding.compute_decode_buffer_size(new ReadOnlySpan<byte>(item_name, 1000))];
        int decoded_length = FhEncoding.decode(new ReadOnlySpan<byte>(item_name, 1000), decoded, flags:FhEncodingFlags.IMPLICIT_END);
        string decoded_string = Encoding.UTF8.GetString(decoded, 0, decoded_length);
        item_name_cache[item_id] = decoded_string;
        return decoded_string;
    }

    public string get_other_item_name(uint item_id) {
        var item_type = (item_id & 0xF000) >> 0xC;
        var id = item_id & 0xFFF;

        if (item_type == 0xC && (id < other_item_names.Length))
            return other_item_names[id];

        if (item_type == 0x3 && overdrive_names.ContainsKey(item_id))
            return overdrive_names[item_id];

        return $"Unnamed item ({item_id})";
    }

    private Dictionary<int, FhGCall.d_CT_Exec>     cached_CT_Execs     = new();
    private Dictionary<int, FhGCall.d_CT_RetInt>   cached_CT_RetInts   = new();
    private Dictionary<int, FhGCall.d_CT_RetFloat> cached_CT_RetFloats = new();

    public FhGCall.d_CT_Exec get_CT_Exec(int id) {
        if (cached_CT_Execs.TryGetValue(id, out var result)) {
            return result;
        }
        AtelCallTargetNamespace nmsp = (AtelCallTargetNamespace)(id >> 0xC);
        AtelCallTarget* internal_ct = nmsp.get_internal() + (id & 0xFFF);

        FhGCall.d_CT_Exec ct = Marshal.GetDelegateForFunctionPointer<FhGCall.d_CT_Exec>(internal_ct->ret_float_func);
        cached_CT_Execs[id] = ct;
        return ct;
    }

    public FhGCall.d_CT_RetInt get_CT_RetInt(int id) {
        if (cached_CT_RetInts.TryGetValue(id, out var result)) {
            return result;
        }
        AtelCallTargetNamespace nmsp = (AtelCallTargetNamespace)(id >> 0xC);
        AtelCallTarget* internal_ct = nmsp.get_internal() + (id & 0xFFF);

        FhGCall.d_CT_RetInt ct = Marshal.GetDelegateForFunctionPointer<FhGCall.d_CT_RetInt>(internal_ct->ret_float_func);
        cached_CT_RetInts[id] = ct;
        return ct;
    }

    public FhGCall.d_CT_RetFloat get_CT_RetFloat(int id) {
        if (cached_CT_RetFloats.TryGetValue(id, out var result)) {
            return result;
        }
        AtelCallTargetNamespace nmsp = (AtelCallTargetNamespace)(id >> 0xC);
        AtelCallTarget* internal_ct = nmsp.get_internal() + (id & 0xFFF);

        FhGCall.d_CT_RetFloat ct = Marshal.GetDelegateForFunctionPointer<FhGCall.d_CT_RetFloat>(internal_ct->ret_float_func);
        cached_CT_RetFloats[id] = ct;
        return ct;
    }

    // Custom namespace
    public void AtelInitTotal() {
        _logger.Debug("Initializing Atel namespaces");
        FhXCall.AtelInitTotal.chain_from(AtelInitTotal).fnptr!();

        AtelSetUpCallFunc(0xF, customNameSpaceHandle.AddrOfPinnedObject());
    }

    enum CustomCallTarget : ushort {
        PUSH_INT = 0xF000,
        PUSH_FLOAT,
        IS_CHARACTER_UNLOCKED,
        HAS_CELESTIAL,
        IS_OTHER_LOCATION_CHECKED,
        IS_TREASURE_LOCATION_CHECKED,
        COLLECTED_PRIMERS,
        SEND_OTHER_LOCATION,
        SEND_PARTY_MEMBER_LOCATION,
        SEND_RECRUIT_LOCATION,
        BLOCK_WARP,
        BLOCK_KILIKA_BOAT_CHOICE,
        RESTORE_INTERACTION,
        SET_AIRSHIP_DESTINATIONS,
        SHOW_AIRSHIP_DESTINATIONS,
        HIDE_AIRSHIP_DESTINATIONS,
        SHOW_CURRENT_AIRSHIP_LOCATION,
        HIDE_CURRENT_AIRSHIP_LOCATION,
        TRANSITION_TO_REGION,
        JUMP,
        JUMP_IF_FALSE,
        JUMP_IF_TRUE,
        OFFSET,
        OFFSET_IF_FALSE,
        OFFSET_IF_TRUE,
        UPDATE_REGION_STATE,
        LOCK_PARTY_MEMBER,
        LOCK_ALL_AEONS,
        IS_GOAL_UNLOCKED,
        REPLACE_ENTRY_POINT,
        RESTORE_ENTRY_POINT,
        LIGHTNING_DODGING,
        JECHT_SPHERE,
        KICKED_BLITZBALL_AWAY,
        CHECK_UNLOCKED_AEONS,
    }

    private GCHandle customNameSpaceHandle;
    private AtelCallTarget[] customNameSpace;


    public int CT_RetInt_PushInt(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        int high = atelStack->pop_int();
        int low  = atelStack->pop_int();
        int result = (high << 16) | low;
        _logger.Debug($"PushInt: ({high}, {low}) => {result}");

        atelStack->push_int(result);
        return 1;
    }

    public int CT_RetInt_PushFloat(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        uint high = (uint)atelStack->pop_int() & 0xFFFF;
        uint low  = (uint)atelStack->pop_int() & 0xFFFF;
        float result = BitConverter.Int32BitsToSingle((int)((high << 16) | low));
        _logger.Debug($"PushFloat: ({high}, {low}) => {result}");

        atelStack->push_float(result);
        return 1;
    }

    public int CT_RetInt_IsCharacterUnlocked(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        int character_id = atelStack->pop_int();
        _logger.Debug($"IsCharacterUnlocked: {id_to_character[character_id]}");

        return is_character_unlocked(character_id) ? 1 : 0;
    }

    public int CT_RetInt_HasCelestialWeapon(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        int character_id = atelStack->pop_int();
        _logger.Debug($"HasCelestialWeapon: {id_to_character[character_id]}");

        foreach (var equip in save_data->equipment) {
            if (equip.exists && equip.owner == character_id && equip.is_celestial) return 1;
        }

        return 0;
    }

    public int CT_RetInt_IsOtherLocationChecked(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        int other_id = atelStack->pop_int();
        _logger.Debug($"IsOtherLocationChecked: {other_id | (long)ArchipelagoClientModule.ArchipelagoLocationType.Other}");

        return _client!.local_checked_locations.Contains(other_id | (long)ArchipelagoClientModule.ArchipelagoLocationType.Other) ? 1 : 0;
    }

    public int CT_RetInt_IsTreasureLocationChecked(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        int other_id = atelStack->pop_int();
        _logger.Debug($"IsTreasureLocationChecked: {other_id | (long)ArchipelagoClientModule.ArchipelagoLocationType.Treasure}");

        return _client!.local_checked_locations.Contains(other_id | (long)ArchipelagoClientModule.ArchipelagoLocationType.Treasure) ? 1 : 0;
    }

    public int CT_RetInt_CollectedPrimers(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        int collected_primers = 0;
        for (int i = 0; i < 26; i++) {
            collected_primers += save_data->unlocked_primers.get_bit(i) ? 1 : 0;
        }
        _logger.Debug($"CollectedPrimers: {collected_primers}");

        return collected_primers;
    }

    public int CT_RetInt_SendOtherLocation(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        int other_id =  atelStack->pop_int();
        if (!(_client!.local_checked_locations.Contains(other_id | (long)ArchipelagoClientModule.ArchipelagoLocationType.Other))) {
            if (ArchipelagoFFXModule.item_locations.other.TryGetValue(other_id, out var item)) {
                if (_client!.sendLocation(other_id, ArchipelagoClientModule.ArchipelagoLocationType.Other)) {
                    obtain_item(item.id);
                }
            }
        }
        return 1;
    }

    public int CT_RetInt_SendPartyMemberLocation(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        int partyMember_id =  atelStack->pop_int();
        if (!(_client!.local_checked_locations.Contains(partyMember_id | (long)ArchipelagoClientModule.ArchipelagoLocationType.PartyMember))) {
            if (ArchipelagoFFXModule.item_locations.party_member.TryGetValue(partyMember_id, out var item)) {
                if (_client!.sendLocation(partyMember_id, ArchipelagoClientModule.ArchipelagoLocationType.PartyMember)) {
                    obtain_item(item.id);
                }
            }
        }
        return 1;
    }

    public int CT_RetInt_SendRecruitLocation(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        int recruit_id =  atelStack->pop_int();
        if (!(_client!.local_checked_locations.Contains(recruit_id | (long)ArchipelagoClientModule.ArchipelagoLocationType.Recruit))) {
            if (ArchipelagoFFXModule.item_locations.recruit.TryGetValue(recruit_id, out var item)) {
                if (_client!.sendLocation(recruit_id, ArchipelagoClientModule.ArchipelagoLocationType.Recruit)) {
                    obtain_item(item.id);
                }
            }
        }
        return 1;
    }

    public int atelTurnAround(Span<byte> dest, float x, float y, float z, ushort current_worker, ushort target_worker, ushort entry_point, float movementSpeed = 34.0f, float rotationSpeed1 = 0.10471976f, float rotationSpeed2 = 0.17453294f) {
        _logger.Debug($"x={x}, y={y}, z={z}");
        uint rotationFlags = 0x0000D002;
        uint motionFlags   = 0x0000C001;
        byte[] script = ((AtelInst[])[ // 5
            //call Common.disablePlayerControl? [005Eh]();
            AtelOp.CALLPOPA.build(0x005E),
            //call Common.stopWorkerMotion [00D1h]();
            AtelOp.CALLPOPA.build(0x00D1),
            ////call ChEvent.setIgnoreNavmesh [500Eh](ignore=true [01h]);
            //AtelOp.PUSHII  .build(0x0001),
            //AtelOp.CALLPOPA.build(0x500E),
            //call Common.setMovementSpeed [006Ch](speed=movementSpeed);
            .. atelPushFloat(movementSpeed),
            AtelOp.CALLPOPA.build(0x006C),
            //call Common.0017(p1=0 [00h]);
            AtelOp.PUSHII  .build(0x0000),
            AtelOp.CALLPOPA.build(0x0017),
            //call Common.setRotationSpeed1 [002Eh](float=rotationSpeed1);
            .. atelPushFloat(rotationSpeed1),
            AtelOp.CALLPOPA.build(0x002E),
            //call Common.setRotationSpeed2 [002Fh](float=rotationSpeed2);
            .. atelPushFloat(rotationSpeed2),
            AtelOp.CALLPOPA.build(0x002F),
            //call Common.setDestination [0015h](x=x, y=y, z=z);
            .. atelPushFloat(x),
            .. atelPushFloat(y),
            .. atelPushFloat(z),
            AtelOp.CALLPOPA.build(0x0015),
            //call Common.setRotationTarget1 [0028h](angle=Common.destinationToYaw [001Fh]());
            AtelOp.CALL    .build(0x001F),
            AtelOp.CALLPOPA.build(0x0028),
            //call Common.startRotation [0019h](activeBits=0 [00h], flags=[b0002, b1000, b4000, b8000] [0000D002h], targetWorker=target_worker [00h]);
            AtelOp.PUSHII  .build(0x0000),
            .. atelPushInt(rotationFlags),
            AtelOp.PUSHII  .build(target_worker),
            AtelOp.CALLPOPA.build(0x0019),
            //call Common.startMotion [0018h](activeBits=0 [00h], flags=[b0001, b4000, b8000] [0000C001h], targetWorker=target_worker [00h]);
            AtelOp.PUSHII  .build(0x0000),
            .. atelPushInt(motionFlags),
            AtelOp.PUSHII  .build(target_worker),
            AtelOp.CALLPOPA.build(0x0018),
            // display customStrings[6]
            .. atelDisplayFieldString(1, 0x8006, 256, 224, 4, 0, 0),
            //call Common.waitForMotion [001Ah]();
            AtelOp.CALLPOPA.build(0x001A),
            //call Common.stopWorkerRotation [0078h](worker=Common.getWorkerIndex [0033h](worker=<Self> [FFFFh]));
            AtelOp.PUSHII  .build(0xFFFF),
            AtelOp.CALL    .build(0x0033),
            AtelOp.CALLPOPA.build(0x0078),
            // call SgEvent.fadeinFromBlack? [4004h](frames=5 [5h]);
            AtelOp.PUSHII  .build(0x0000),
            AtelOp.CALLPOPA.build(0x4004),
            // Restore entry point
            AtelOp.PUSHII  .build(target_worker),
            AtelOp.PUSHII  .build(entry_point),
            AtelOp.CALLPOPA.build((ushort)CustomCallTarget.RESTORE_ENTRY_POINT),
            // Restore cross
            AtelOp.PUSHII  .build(current_worker),
            AtelOp.CALLPOPA.build((ushort)CustomCallTarget.RESTORE_INTERACTION),
            //call ChEvent.setIgnoreNavmesh [500Eh](ignore=false [00h]);
            AtelOp.PUSHII  .build(0x0000),
            AtelOp.CALLPOPA.build(0x500E),
            // call Common.waitForText [0084h](boxIndex=1 [01h], p2=1 [01h]);
            AtelOp.PUSHII  .build(0x0001),
            AtelOp.PUSHII  .build(0x0001),
            AtelOp.CALLPOPA.build(0x0084),
            //call Common.enablePlayerControl? [005Dh]();
            AtelOp.CALLPOPA.build(0x005D),
            //return;
            AtelOp.RET     .build(      ),

        ]).SelectMany(x => x.to_bytes()).ToArray();

        if (dest.Length >= script.Length) {
            script.CopyTo(dest);
        }
        return script.Length;
    }
    private bool  savedCrossInteractionStatus;
    private byte  savedInteractionFlags;

    public int CT_RetInt_BlockWarp(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        return blockWarp(work, storage, atelStack);
    }

    public int CT_RetInt_BlockKilikaBoatChoice(AtelBasicWorker* worker, int* storage, AtelStack* stack) {
        return blockKilikaBoatChoice(worker, storage, stack);
    }

    public int blockWarp(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        int entrance = atelStack->pop_int();
        int map      = atelStack->pop_int();

        _logger.Info("Exit leads to other Region!");

        Chr* tidus = (Chr*)((int)Atel.controllers[0].worker(0) + 0x9c);
        if (tidus != null && tidus->actor != null) {
            Vector3 playerPos = tidus->actor->chr_pos_vec.AsVector3();
            var entrances = Atel.controllers[0].worker(0)->script_chunk->map_entrances.ToArray();
            var closestEntrance = Atel.controllers[0].worker(0)->script_chunk->map_entrances.ToArray()
                        .Select((e, i) => new {Index=i, Entrance=e, Distance=(playerPos - e.pos).Length()})
                        .MinBy(tuple => tuple.Distance);

            if (closestEntrance?.Distance < 200) {
                _logger.Debug($"Entrance within 200: pos:({closestEntrance.Entrance.x}, {closestEntrance.Entrance.y}, {closestEntrance.Entrance.z}) distance:{closestEntrance.Distance}");
                Vector3 target_pos = closestEntrance.Entrance.pos;

                ushort entry_point = (ushort)(Atel.controllers[0].worker(0)->script_header->entry_point_count - 1);

                if (turnAroundScript == null) turnAroundScript = (byte*)NativeMemory.AllocZeroed((uint)turnAroundScriptLength);
                atelTurnAround(new Span<byte>(turnAroundScript, turnAroundScriptLength), target_pos.X, target_pos.Y, target_pos.Z, work->worker_idx, 0, entry_point, 17, 1f, 1f);


                AtelBasicWorker* targetWorker = Atel.controllers[0].worker(0);
                if (!originalEntryPoints.ContainsKey((0, entry_point))) {
                    originalEntryPoints[(0, entry_point)] = targetWorker->table_entry_points[entry_point];
                }
                int targetAddress = (int)turnAroundScript;
                int addressOffset = targetAddress - (int)targetWorker->code_ptr;
                targetWorker->table_entry_points[entry_point] = (uint)addressOffset;

                // Disable cross interaction
                savedInteractionFlags = work->field_interaction_flags;
                work->field_interaction_flags = 0;
                //savedCrossInteractionStatus = (work->field_interaction_flags & (1 << 2)) != 0;
                //if (savedCrossInteractionStatus) work->field_interaction_flags = (byte)(work->field_interaction_flags & ~(1 << 2));

                atelStack->push_int(2); // signal priority?
                atelStack->push_int(0); // worker
                atelStack->push_int(entry_point); // entrypoint
                FhXCall.FUN_00867370.fnptr!((byte)AtelOp.REQEW & 0x7F, work, &work->threads[work->current_thread_priority], atelStack, 0);
                work->__0x34 = (ushort)(work->__0x34 & 0xEBFF | 0x800);
                //work->__0x34 = (ushort)(work->__0x34 | 0x800);
                atelStack->pop_int();
                //FhXCall.FUN_008671d0.fnptr!((byte)AtelOp.REQ & 0x7F, &work->threads[work->current_thread_priority], work, atelStack);
                return 1;
            }
            _logger.Debug($"Closest entrance: pos:({closestEntrance?.Entrance.x}, {closestEntrance?.Entrance.y}, {closestEntrance?.Entrance.z}) distance:{closestEntrance?.Distance}");
        }

        // Warp
        atelStack->push_int(382);
        atelStack->push_int(0);
        Common_warpToMap(work, storage, atelStack);
        return 1;
    }

    public int blockKilikaBoatChoice(AtelBasicWorker* worker, int* storage, AtelStack* stack) {
        uint choice = (uint)worker->current_thread.reg_a;

        if (choice == 0) {
            // Chose Kilika->Besaid
            worker->table_event_data[0] = 2; // 2 to cancel
        } else {
            worker->table_event_data[0] = choice; // Replicate the code that we're replacing
        }

        return 1;
    }

    public int CT_RetInt_RestoreInteraction(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        int target_worker_index = atelStack->pop_int();
        AtelBasicWorker* target_worker = Atel.controllers[0].worker(target_worker_index);
        //if (savedCrossInteractionStatus) target_worker->field_interaction_flags |= 1 << 2; // Enable cross interaction
        target_worker->field_interaction_flags = savedInteractionFlags;
        return 1;
    }

    public  int CT_RetInt_SetAirshipDestinations(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        set_airship_destinations();
        return 1;
    }

    public int CT_RetInt_ShowAirshipDestinations(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        var var_table = work->table_event_data;
        uint* airshipDestinations = &var_table[13];
        uint airshipDestinationsOffset = var_table[50];
        uint airshipDestinationsAbsoluteCursor = var_table[2];

        for (uint i = 0; i < 15; i++) {
            uint destination_index = airshipDestinations[i+airshipDestinationsOffset];
            if (destination_index != 0) {
                customStringDrawInfos[$"destination {i}"] = new CustomStringDrawInfo(airship_destination_names[destination_index], new(57f, 108f + 16.857f * i), 1f);
            }
            else {
                customStringDrawInfos.Remove($"destination {i}");
            }
        }

        // Set eventVar0018 = (18 [12h] * (eventVar0008 - eventVar00C8)) + 84 [54h];
        var_table[6] = (18 * (airshipDestinationsAbsoluteCursor - airshipDestinationsOffset)) + 84;
        return 1;
    }

    public int CT_RetInt_HideAirshipDestinations(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        for (uint i = 0; i < 15; i++) {
            customStringDrawInfos.Remove($"destination {i}");
        }
        return 1;
    }

    public int CT_RetInt_ShowCurrentAirshipLocation(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        _logger.Debug("ShowCurrentAirshipLocation");
        customStringDrawInfos[$"current destination"] = new CustomStringDrawInfo(airship_destination_names[save_data->current_airship_location], new(52f, 73f), 1f);

        // AE0300 D80F80 call Map.?show2DLayer [800Fh](layerIndex=3 [03h]);
        atelStack->push_int(3);
        h_Map_show2DLayerResultInt.fnptr!(work, storage, atelStack);
        return 1;
    }

    public int CT_RetInt_HideCurrentAirshipLocation(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        _logger.Debug("HideCurrentAirshipLocation");
        customStringDrawInfos.Remove($"current destination");

        // AE0300 D81080 call Map.?hide2DLayer [8010h](layerIndex=3 [03h]);
        atelStack->push_int(3);
        h_Map_hide2DLayerResultInt.fnptr!(work, storage, atelStack);
        return 1;
    }

    public int CT_RetInt_TransitionToRegion(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        RegionEnum region = (RegionEnum)atelStack->pop_int();

        if (current_region != RegionEnum.None) {
            update_region_state();
            current_region = RegionEnum.None;
            skip_state_updates = false;
        }

        current_region = region;
        int map = 0, entrance = 0;
        restore_region_state(ref map, ref entrance);
        _logger.Debug($"Entering {region}: map={map}, entrance={entrance}");

        // Unlock locked characters. May not be necessary
        foreach (var character in locked_characters) {
            locked_characters[character.Key] = false;
        }
        save_party();
        reset_party();

        atelStack->push_int(map);
        atelStack->push_int(entrance);
        Common_transitionToMap(work, storage, atelStack);

        return 1;
    }

    public int CT_RetInt_Jump(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        int jump_index = atelStack->pop_int();
        work->current_thread.pc = (byte*)customScriptHandles[jump_index].AddrOfPinnedObject() - 3;
        return 1;
    }

    public int CT_RetInt_JumpIfFalse(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        int jump_index = atelStack->pop_int();
        int val = atelStack->pop_int();
        if (val == 0) {
            work->current_thread.pc = (byte*)customScriptHandles[jump_index].AddrOfPinnedObject() - 3;
        }
        return 1;
    }

    public int CT_RetInt_JumpIfTrue(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        int jump_index = atelStack->pop_int();
        int val = atelStack->pop_int();
        if (val == 1) {
            work->current_thread.pc = (byte*)customScriptHandles[jump_index].AddrOfPinnedObject() - 3;
        }
        return 1;
    }

    public int CT_RetInt_Offset(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        int offset = atelStack->pop_int();
        work->current_thread.pc += offset;
        return 1;
    }

    public int CT_RetInt_OffsetIfFalse(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        int offset = atelStack->pop_int();
        int val = atelStack->pop_int();
        if (val == 0) {
            work->current_thread.pc += offset;
        }
        return 1;
    }

    public int CT_RetInt_OffsetIfTrue(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        int offset = atelStack->pop_int();
        int val = atelStack->pop_int();
        if (val == 1) {
            work->current_thread.pc += offset;
        }
        return 1;
    }

    public int CT_RetInt_UpdateRegionState(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        Vector3 playerPos = actors->chr_pos_vec.AsVector3();
        var closestEntrance = Atel.controllers[0].worker(0)->script_chunk->map_entrances.ToArray()
                    .Select((entrance, index) => new {Index=index, Entrance=entrance, Distance=(playerPos - entrance.pos).Length()})
                    .MinBy(tuple => tuple.Distance);
        if (closestEntrance?.Distance < 100) {
            save_data->current_spawnpoint = (byte)closestEntrance.Index;
            _logger.Debug($"Entrance within 100: pos:({closestEntrance.Entrance.x}, {closestEntrance.Entrance.y}, {closestEntrance.Entrance.z}) distance:{closestEntrance.Distance}");
        }

        update_region_state(save_data->current_room_id, save_data->current_spawnpoint);
        refill_inventory();
        return 1;
    }

    public int CT_RetInt_LockPartyMember(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        int party_member = atelStack->pop_int();
        locked_characters[party_member] = true;
        if (party_member == PlySaveId.PC_MAGUS1) {
            locked_characters[PlySaveId.PC_MAGUS2] = true;
            locked_characters[PlySaveId.PC_MAGUS3] = true;
        }
        return 1;
    }

    public int CT_RetInt_LockAllAeons(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        for (int chr_id = PlySaveId.PC_VALEFOR; chr_id <= PlySaveId.PC_MAGUS3; chr_id++) {
            locked_characters[chr_id] = true;
        }

        return 1;
    }

    public int CT_RetInt_IsGoalUnlocked(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        bool goal_requirement = false;
        bool primer_requirement = false;

        switch (seed.Options.GoalRequirement) {
            case GoalRequirement.None:
                goal_requirement = true;
                break;
            case GoalRequirement.PartyMembers:
                if (unlocked_characters.Count(x => x.Key < 8 && x.Value) >= Math.Min(seed.Options.RequiredPartyMembers, 8))
                    goal_requirement = true;
                //if (unlocked_characters.All(c => c.Value)) {
                //    return 1;
                //}
                break;
            case GoalRequirement.PartyMembersAndAeons:
                if (unlocked_characters.Count(x => x.Key < 16 && x.Value) >= seed.Options.RequiredPartyMembers)
                    goal_requirement = true;
                break;
            case GoalRequirement.Pilgrimage:
                if (
                    _client!.local_checked_locations.Contains(8 | (long)ArchipelagoClientModule.ArchipelagoLocationType.PartyMember)
                 && _client!.local_checked_locations.Contains(9 | (long)ArchipelagoClientModule.ArchipelagoLocationType.PartyMember)
                 && _client!.local_checked_locations.Contains(10 | (long)ArchipelagoClientModule.ArchipelagoLocationType.PartyMember)
                 && _client!.local_checked_locations.Contains(11 | (long)ArchipelagoClientModule.ArchipelagoLocationType.PartyMember)
                 && _client!.local_checked_locations.Contains(12 | (long)ArchipelagoClientModule.ArchipelagoLocationType.PartyMember)
                 && _client!.local_checked_locations.Contains(37 | (long)ArchipelagoClientModule.ArchipelagoLocationType.Boss)
                 ) {
                    goal_requirement = true;
                }
                break;
            case GoalRequirement.Nemesis:
                if (_client!.local_checked_locations.Contains(83 | (long)ArchipelagoClientModule.ArchipelagoLocationType.Boss)) {
                    goal_requirement = true;
                }
                break;
        }

        if (seed.Options.RequiredPrimers > 0) {
            int collected_primers = 0;
            for (int i = 0; i < 26; i++) {
                collected_primers += save_data->unlocked_primers.get_bit(i) ? 1 : 0;
            }

            if (collected_primers >= seed.Options.RequiredPrimers)
                primer_requirement = true;
        }
        else
            primer_requirement = true;


        return goal_requirement && primer_requirement ? 1 : 0;
    }

    public int CT_RetInt_ReplaceEntryPoint(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        int script_index = atelStack->pop_int();
        int entryPoint   = atelStack->pop_int();
        int workerIndex  = atelStack->pop_int();

        AtelBasicWorker* targetWorker = Atel.controllers[0].worker(workerIndex);

        if (!originalEntryPoints.ContainsKey((workerIndex, entryPoint))) {
            originalEntryPoints[(workerIndex, entryPoint)] = targetWorker->table_entry_points[entryPoint];
        }

        int targetAddress = (int)customScriptHandles[script_index].AddrOfPinnedObject();

        int addressOffset = targetAddress - (int)targetWorker->code_ptr;

        targetWorker->table_entry_points[entryPoint] = (uint)addressOffset;
        return 1;
    }

    public int CT_RetInt_RestoreEntryPoint(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        int entryPoint = atelStack->pop_int();
        int workerIndex = atelStack->pop_int();
        if (originalEntryPoints.TryGetValue((workerIndex, entryPoint), out uint value)) {
            Atel.controllers[0].worker(workerIndex)->table_entry_points[entryPoint] = value;
        }
        return 1;
    }

    public int CT_RetInt_LightningDodging(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        int southNorth = atelStack->pop_int();
        ushort highestDodged = save_data->lightning_dodging_highest_consecutive_dodges;
        byte* table = (byte*)work->table_event_data;
        byte* dodged;
        byte* streak;

        // Get relevant eventVars per room
        if (southNorth == 0) {
            dodged = &table[0x0007];
            streak = &table[0x0012];
        } else {
            dodged = &table[0x0009];
            streak = &table[0x000E];
        }

        // Free & Remove existing strings

        // Did the player dodge?
        int current;
        if (*dodged == 0)
            current = 0;
        else
            current = *streak + 1;

        string currentString = $"Dodged: {current}";
        customStringDrawInfos["Lightning Streak"] = new CustomStringDrawInfo(new ManagedCustomString(currentString), new(40f, 140f), 0.5f);
        _logger.Info($"Lightning Dodged: {current}");

        if (current > save_data->lightning_dodging_highest_consecutive_dodges)
            highestDodged = (ushort)current;

        string highestString = $"Highest: {highestDodged}";
        customStringDrawInfos["Lightning Highest Streak"] = new CustomStringDrawInfo(new ManagedCustomString(highestString), new(40f, 150f), 0.5f);
        _logger.Info($"Highest Consecutive Dodged: {highestDodged}");

        return *dodged == 1 ? 1 : 0;
    }

    public int CT_RetInt_JechtSphere(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        int jecht_sphere = atelStack->pop_int();
        int other_id =  atelStack->pop_int();

        if (!(_client!.local_checked_locations.Contains(other_id | (long)ArchipelagoClientModule.ArchipelagoLocationType.Other))) {
            if (ArchipelagoFFXModule.item_locations.other.TryGetValue(other_id, out var item)) {
                if (_client!.sendLocation(other_id, ArchipelagoClientModule.ArchipelagoLocationType.Other)) {
                    obtain_item(item.id);
                }
            }
        }
        save_data->jecht_spheres.flags_spheres_seen.set_bit(jecht_sphere, true);

        return 1;
    }

    public int CT_RetInt_KickedBlitzballAway(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        byte* table = (byte*)work->table_event_data;
        table[0x0008] = 1;

        int treasure_id = 312;
        if (!(_client!.local_checked_locations.Contains(treasure_id | (long)ArchipelagoClientModule.ArchipelagoLocationType.Treasure))) {
            if (ArchipelagoFFXModule.item_locations.treasure.TryGetValue(treasure_id, out var item)) {
                lock (_client!.client_lock) {
                    if (_client!.is_connected) {
                        _client!.current_session!.Locations.ScoutLocationsAsync(Archipelago.MultiClient.Net.Enums.HintCreationPolicy.CreateAndAnnounceOnce,
                                                                                            treasure_id | (long)ArchipelagoClientModule.ArchipelagoLocationType.Treasure);
                    }
                }
                _client!.SayAsync($"kicked away {item.player}'s {item.name}!");
            }
        }
        return 1;
    }

    public int CT_RetInt_CheckUnlockedAeons(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        uint aeon_bitfield = work->table_priv_data[0];

        _logger.Info($"aeon_bitfield: {aeon_bitfield}");

        uint temp = aeon_bitfield;
        for (int i = 0; i < PlySaveId.PC_DUMMY; i++) {
            _logger.Info($"{id_to_character[i]}: {(temp & 1) == 1}");
            temp >>= 1;
        }

        if (aeon_bitfield != 0) {
            return 1;
        }

        if (!(_client!.local_checked_locations.Contains(41 | (long)ArchipelagoClientModule.ArchipelagoLocationType.Boss))) {
            if (ArchipelagoFFXModule.item_locations.boss.TryGetValue(41, out var item)) {
                if (_client!.sendLocation(41, ArchipelagoClientModule.ArchipelagoLocationType.Boss)) {
                    obtain_item(item.id);
                }
            }
        }
        return 0;
    }

    public struct CustomStringDrawInfo(ManagedCustomString customString, Vector2 pos, float scale = 0.65f, byte color = 0, bool persistent = false) {
        public ManagedCustomString customString = customString;
        public Vector2             pos          = pos;
        public float               scale        = scale;
        public byte                color        = color;
        public bool                persistent   = persistent;
    }

    public Dictionary<string, CustomStringDrawInfo> customStringDrawInfos = [];

    public void render_game() {
        FhXCall.TODrawWindow.chain_from(render_game).fnptr!();

        foreach ((string key, CustomStringDrawInfo drawInfo) in customStringDrawInfos) {
            fixed (byte* text = drawInfo.customString.encoded) {
                FhXCall.TOMkpCrossExtMesFontLClutTypeRGBA.fnptr!(0, text, drawInfo.pos.X, drawInfo.pos.Y, drawInfo.color, 0, 0x80, 0x80, 0x80, 0x80, drawInfo.scale, 0);
            }
        }
    }
}

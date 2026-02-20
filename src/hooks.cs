using Fahrenheit.Core;
using Fahrenheit.Core.Atel;
using Fahrenheit.Core.FFX;
using Fahrenheit.Core.FFX.Battle;
using Fahrenheit.Core.FFX.Ids;
using Fahrenheit.Modules.ArchipelagoFFX.Client;
using Fahrenheit.Modules.ArchipelagoFFX.GUI;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using static Fahrenheit.Core.FFX.Globals;
using static Fahrenheit.Modules.ArchipelagoFFX.ArchipelagoData;
using static Fahrenheit.Modules.ArchipelagoFFX.Client.FFXArchipelagoClient;
using static Fahrenheit.Modules.ArchipelagoFFX.delegates;
using Color = Archipelago.MultiClient.Net.Models.Color;
using Scope = Archipelago.MultiClient.Net.Enums.Scope;

namespace Fahrenheit.Modules.ArchipelagoFFX;
public unsafe partial class ArchipelagoFFXModule {

    public static int* takara_pointer => FhUtil.ptr_at<int>(0xD35FEC);
    public static int* buki_get_pointer => FhUtil.ptr_at<int>(0xD35FF4);


    // AtelEventSetUp
    private static FhMethodHandle<AtelEventSetUp> _AtelEventSetUp;

    public static char* get_event_name(uint event_id) => FhUtil.get_fptr<AtelGetEventName>(0x4796e0)(event_id);
    public static int atel_stack_pop(int* param_1, AtelStack* atelStack) => FhUtil.get_fptr<AtelStackPop>(0x0046de90)(param_1, atelStack);

    private static FhMethodHandle<Common_obtainTreasureInit> _Common_obtainTreasureInit;
    private static FhMethodHandle<Common_obtainTreasureSilentlyInit> _Common_obtainTreasureSilentlyInit;
    private static FhMethodHandle<CT_RetInt_01B6> _Common_isBrotherhoodUnpoweredRetInt;
    private static FhMethodHandle<Common_upgradeBrotherhoodRetInt> _Common_upgradeBrotherhoodRetInt;
    private static FhMethodHandle<Common_obtainBrotherhoodRetInt> _Common_obtainBrotherhoodRetInt;
    //private static FhMethodHandle<Common_grantCelestialUpgrade> _Common_grantCelestialUpgrade;
    private static FhMethodHandle<Common_setPrimerCollected> _Common_setPrimerCollected;
    private static FhMethodHandle<Common_transitionToMap> _Common_transitionToMap;
    private static FhMethodHandle<Common_warpToMap> _Common_warpToMap;

    private static FhMethodHandle<TkSetLegendAbility> _TkSetLegendAbility;

    private static FhMethodHandle<SgEvent_showModularMenuInit> _SgEvent_showModularMenuInit;

    //private static FhMethodHandle<Common_playFieldVoiceLineInit> _Common_playFieldVoiceLineInit;
    //private static FhMethodHandle<Common_playFieldVoiceLineExec> _Common_playFieldVoiceLineExec;
    //private static FhMethodHandle<Common_playFieldVoiceLineResultInt> _Common_playFieldVoiceLineResultInt;



    private static Common_playFieldVoiceLineInit _Common_playFieldVoiceLineInit;
    private static Common_playFieldVoiceLineExec _Common_playFieldVoiceLineExec;
    private static Common_playFieldVoiceLineResultInt _Common_playFieldVoiceLineResultInt;

    private static Common_00D6Init _Common_00D6Init;
    private static Common_00D6Exec _Common_00D6Exec;
    private static Common_00D6ResultInt _Common_00D6ResultInt;


    // Common.01D1Init
    private static Common_01D1Init _Common_01D1Init;
    // Common.01D1Exec
    private static Common_01D1Exec _Common_01D1Exec;

    private static FhMethodHandle<Common_addPartyMember> _Common_addPartyMember;
    private static FhMethodHandle<Common_removePartyMember> _Common_removePartyMember;
    private static FhMethodHandle<Common_removePartyMemberLongTerm> _Common_removePartyMemberLongTerm;
    private static FhMethodHandle<Common_setWeaponInvisible> _Common_setWeaponVisibilty;
    private static FhMethodHandle<Common_putPartyMemberInSlot> _Common_putPartyMemberInSlot;

    private static FhMethodHandle<Common_pushParty> _Common_pushParty;
    private static FhMethodHandle<Common_popParty> _Common_popParty;

    // getCurrentPartySlots
    private static FhMethodHandle<MsGetSavePartyMember> _MsGetSavePartyMember;

    // Battle releated
    private static MsBtlListGroup _MsBtlListGroup;
    private static FhMethodHandle<MsBattleExe> _MsBattleExe;
    private static FhMethodHandle<MsMonsterCapture> _MsMonsterCapture;
    public static MsBattleLabelExe _MsBattleLabelExe;
    private static FhMethodHandle<FUN_00791820> _FUN_00791820;
    public static FhMethodHandle<FUN_00783bb0> _FUN_00783bb0;
    public static MsGetMon _MsGetMon;

    private static FhMethodHandle<MsBtlGetPos> _MsBtlGetPos;

    private static FhMethodHandle<MsBtlReadSetScene> _MsBtlReadSetScene;

    // giveItem
    private static FhMethodHandle<FUN_007905a0> _FUN_007905a0;
    // giveKeyItem
    // takeGil


    // readFromBin
    private static FhMethodHandle<FUN_007ab890> _FUN_007ab890;

    // getWeaponName
    private static FhMethodHandle<FUN_007a0d10> _FUN_007a0d10;
    // getWeaponModel
    private static FhMethodHandle<FUN_007a0c70> _FUN_007a0c70;
    // obtainTreasureCleanup
    private static FhMethodHandle<FUN_007993f0> _FUN_007993f0;

    private static MsGetChr _MsGetChr;
    private static FhCall.MsGetComData _MsGetComData;
    private static FhCall.MsGetCommandUse _MsGetCommandUse;
    private static FhCall.MsGetCommandMP _MsGetCommandMP;
    private static FhCall.MsGetRamChrMonster _MsGetRamChrMonster;
    private static FhCall.TODrawCrossBoxXYWHC2 _TODrawCrossBoxXYWHC2;


    private static FhMethodHandle<TkMenuAppearMainCmdWindow> _TkMenuAppearMainCmdWindow;

    // Sphere Grid Experiment
    private static FhMethodHandle<eiAbmParaGet>     _eiAbmParaGet;
    private static FhMethodHandle<MsSetSaveParam>   _MsSetSaveParam;
    private static FhMethodHandle<MsSetRamChrParam> _MsSetRamChrParam;
    private static FhMethodHandle<FUN_00a48910>     _FUN_00a48910;

    private static FhMethodHandle<MsApUp> _MsApUp;


    private static FhMethodHandle<openFile> _openFile;
    private static readFile _readFile;
    private static FhMethodHandle<FUN_0070aec0> _FUN_0070aec0;

    private static FhCall.ChN_ReadSystemMGRP _ChN_ReadSystemMGRP;
    private static Common_loadModel _Common_loadModel;
    private static Common_0043 _Common_0043;
    private static Common_linkFieldToBattleActor _Common_linkFieldToBattleActor;
    private static FhCall.SndSepPlay _SndSepPlay;


    public static MsGetExcelData _MsGetExcelData;

    private static FhMethodHandle<Map_800F> _Map_800F;

    private static FhMethodHandle<FUN_0086bec0> _FUN_0086bec0;
    private static FhMethodHandle<FUN_0086bea0> _FUN_0086bea0;

    private static FhMethodHandle<graphicInitFMVPlayer> _graphicInitFMVPlayer;

    private static FhMethodHandle<FUN_00656c90> _FUN_00656c90;
    private static FUN_0065ee30 _FUN_0065ee30;
    private static ClusterManager_loadPCluster _ClusterManager_loadPCluster;
    private static Phyre_PFramework_PApplication_FixupClusters _Phyre_PFramework_PApplication_FixupClusters;
    private static ClusterManager_releasePCluster _ClusterManager_releasePCluster;
    private static ClusterManager_getPClusterByName _ClusterManager_getPClusterByName;
    private static fiosUnifyFilename _fiosUnifyFilename;


    // ObtainTreasure related
    private static FhMethodHandle<TkMsImportantSet> _TkMsImportantSet;
    private static MsPayGIL _MsPayGIL;
    private static SndSepPlaySimple _SndSepPlaySimple;
    private static MsGetSaveWeapon _MsGetSaveWeapon;
    private static FUN_007ab930 _FUN_007ab930; // giveWeapon?
    private static AtelGetCurCtrlWork _AtelGetCurCtrlWork;
    private static MsFieldItemGet _MsFieldItemGet;
    private static TkMsGetRomItem _TkMsGetRomItem;
    private static MsSaveItemUse _MsSaveItemUse;
    private static MsImportantName _MsImportantName;
    private static CT_RetInt_0065 _CT_RetInt_0065;
    private static CT_RetInt_006A _CT_RetInt_006A;
    private static FhCall.MsBtlGetInit _MsBtlGetInit;
    private static AtelGetMesWinWork _AtelGetMesWinWork;
    private static FUN_008b8910 _FUN_008b8910; // setMessageWindowVariableType? (0: text*, 1: int)
    private static FUN_008bda20 _FUN_008bda20; // getMenuText
    private static FUN_008b8930 _FUN_008b8930; // setMessageWindowVariable
    private static FUN_0086a0c0 _FUN_0086a0c0;


    // Voice related
    private static FhMethodHandle<FmodVoice_dataChange> _FmodVoice_dataChange;
    private static FMOD_EventSystem_load _FMOD_EventSystem_load;

    private static FhMethodHandle<FfxFmod_soundInit_setLang> _FfxFmod_soundInit_setLang;
    private static FhMethodHandle<LocalizationManager_Initialize> _LocalizationManager_Initialize;
    public static LocalizationManager_GetInstance _LocalizationManager_GetInstance;
    public static FfxFmod_soundInit _FfxFmod_soundInit;
    public static FmodVoice_initList _FmodVoice_initList;


    // Custom namespace
    private static FhMethodHandle<AtelInitTotal> _AtelInitTotal;
    public static void AtelSetUpCallFunc(int id, nint nameSpacePtr) => FhUtil.get_fptr<AtelSetUpCallFunc>(__addr_AtelSetUpCallFunc)(id, nameSpacePtr);

    // Airship menu related
    public static FUN_00867370 _FUN_00867370;
    public static FUN_008671d0 _FUN_008671d0;
    public static Map_show2DLayerResultInt _Map_show2DLayerResultInt;
    public static Map_hide2DLayerResultInt _Map_hide2DLayerResultInt;

    // Drawing
    public static TOMkpCrossExtMesFontLClutTypeRGBA _TOMkpCrossExtMesFontLClutTypeRGBA;
    public static ToMakeBtlEasyFont _ToMakeBtlEasyFont;

    public void init_hooks() {
        const string game = "FFX.exe";

        _AtelEventSetUp = new FhMethodHandle<AtelEventSetUp>(this, game, __addr_AtelEventSetUp, h_AtelEventSetUp);

        _Common_obtainTreasureInit = new FhMethodHandle<Common_obtainTreasureInit>(this, game, __addr_Common_obtainTreasureInit, h_Common_obtainTreasureInit);

        _Common_obtainTreasureSilentlyInit = new FhMethodHandle<Common_obtainTreasureSilentlyInit>(this, game, __addr_Common_obtainTreasureSilentlyInit, h_Common_obtainTreasureSilentlyInit);

        _Common_isBrotherhoodUnpoweredRetInt = new FhMethodHandle<CT_RetInt_01B6>(this, game, __addr_CT_RetInt_01B6, h_Common_isBrotherhoodUnpoweredRetInt);
        _Common_upgradeBrotherhoodRetInt = new FhMethodHandle<Common_upgradeBrotherhoodRetInt>(this, game, __addr_CT_RetInt_01B7, h_Common_upgradeBrotherhoodRetInt);
        _Common_obtainBrotherhoodRetInt = new FhMethodHandle<Common_obtainBrotherhoodRetInt>(this, game, __addr_Common_obtainBrotherhoodRetInt, h_Common_obtainBrotherhoodRetInt);

        _TkSetLegendAbility = new FhMethodHandle<TkSetLegendAbility>(this, game, __addr_TkSetLegendAbility, h_TkSetLegendAbility);

        _Common_setPrimerCollected = new FhMethodHandle<Common_setPrimerCollected>(this, game, __addr_Common_setPrimerCollected, h_Common_setPrimerCollected);

        _Common_transitionToMap = new FhMethodHandle<Common_transitionToMap>(this, game, __addr_Common_transitionToMap, h_Common_transitionToMap);
        _Common_warpToMap = new FhMethodHandle<Common_warpToMap>(this, game, __addr_Common_warpToMap, h_Common_warpToMap);

        _SgEvent_showModularMenuInit = new FhMethodHandle<SgEvent_showModularMenuInit>(this, game, __addr_SgEvent_showModularMenuInit, h_SgEvent_showModularMenuInit);


        //_Common_playFieldVoiceLineInit = new FhMethodHandle<Common_playFieldVoiceLineInit>(this, game, h_Common_playFieldVoiceLineInit, offset: 0x0045cb70);
        //_Common_playFieldVoiceLineExec = new FhMethodHandle<Common_playFieldVoiceLineExec>(this, game, h_Common_playFieldVoiceLineExec, offset: 0x0045cd30);
        //_Common_playFieldVoiceLineResultInt = new FhMethodHandle<Common_playFieldVoiceLineResultInt>(this, game, h_Common_playFieldVoiceLineResultInt, offset: 0x0045d150);

        _Common_playFieldVoiceLineInit      = FhUtil.get_fptr<Common_playFieldVoiceLineInit>     (__addr_Common_playFieldVoiceLineInit     );
        _Common_playFieldVoiceLineExec      = FhUtil.get_fptr<Common_playFieldVoiceLineExec>     (__addr_Common_playFieldVoiceLineExec     );
        _Common_playFieldVoiceLineResultInt = FhUtil.get_fptr<Common_playFieldVoiceLineResultInt>(__addr_Common_playFieldVoiceLineResultInt);

        _Common_00D6Init      = FhUtil.get_fptr<Common_00D6Init>     (__addr_Common_00D6Init     );
        _Common_00D6Exec      = FhUtil.get_fptr<Common_00D6Exec>     (__addr_Common_00D6Exec     );
        _Common_00D6ResultInt = FhUtil.get_fptr<Common_00D6ResultInt>(__addr_Common_00D6ResultInt);



        // Common.01D1Init
        //_FUN_0085fb60 = new FhMethodHandle<FUN_0085fb60>(this, game, h_after_voiceline_init, offset: 0x0045fb60);
        // Common.01D1Exec
        //_FUN_0085fdb0 = new FhMethodHandle<FUN_0085fdb0>(this, game, h_after_voiceline_exec, offset: 0x0045fdb0);

        // Common.01D1Init
        _Common_01D1Init = FhUtil.get_fptr<Common_01D1Init>(__addr_Common_01D1Init);
        // Common.01D1Exec
        _Common_01D1Exec = FhUtil.get_fptr<Common_01D1Exec>(__addr_Common_01D1Exec);




        _Common_addPartyMember = new FhMethodHandle<Common_addPartyMember>(this, game, 0x0045b5a0, h_Common_addPartyMember);
        _Common_removePartyMember = new FhMethodHandle<Common_removePartyMember>(this, game, 0x0045b6c0, h_Common_removePartyMember);
        _Common_removePartyMemberLongTerm = new FhMethodHandle<Common_removePartyMemberLongTerm>(this, game, 0x0045aaf0, h_Common_removePartyMemberLongTerm);
        _Common_setWeaponVisibilty = new FhMethodHandle<Common_setWeaponInvisible>(this, game, 0x00456770, h_Common_setWeaponInvisible);
        _Common_putPartyMemberInSlot = new FhMethodHandle<Common_putPartyMemberInSlot>(this, game, 0x0045bc90, h_Common_putPartyMemberInSlot);


        _Common_pushParty = new FhMethodHandle<Common_pushParty>(this, game, 0x0045b350, h_Common_pushParty);
        _Common_popParty = new FhMethodHandle<Common_popParty>(this, game, 0x0045b3c0, h_Common_popParty);

        // getCurrentPartySlots
        _MsGetSavePartyMember = new FhMethodHandle<MsGetSavePartyMember>(this, game, __addr_MsGetSavePartyMember, h_MsGetSavePartyMember);


        _MsBtlListGroup = FhUtil.get_fptr<MsBtlListGroup>(__addr_MsBtlListGroup);
        _MsBattleExe = new FhMethodHandle<MsBattleExe>(this, game, __addr_MsBattleExe, h_MsBattleExe);
        _MsBattleLabelExe = FhUtil.get_fptr<MsBattleLabelExe>(__addr_MsBattleLabelExe);
        _FUN_00791820 = new FhMethodHandle<FUN_00791820>(this, game, 0x00391820, h_FUN_00791820);

        _MsBtlGetPos = new FhMethodHandle<MsBtlGetPos>(this, game, 0x003ac000, h_MsBtlGetPos);

        _MsBtlReadSetScene = new FhMethodHandle<MsBtlReadSetScene>(this, game, 0x00383ed0, h_MsBtlReadSetScene);

        _MsMonsterCapture = new FhMethodHandle<MsMonsterCapture>(this, game, __addr_MsMonsterCapture, h_MsMonsterCapture);

        _FUN_00783bb0 = new FhMethodHandle<FUN_00783bb0>(this, game, __addr_FUN_00783bb0, h_FUN_00783bb0);
        _MsGetMon = FhUtil.get_fptr<MsGetMon>(__addr_MsGetMon);


        // giveItem
        _FUN_007905a0 = new FhMethodHandle<FUN_007905a0>(this, game, 0x003905a0, h_give_item);



        // readFromBin
        _FUN_007ab890 = new FhMethodHandle<FUN_007ab890>(this, game, 0x003ab890, h_read_from_bin);

        // getWeaponName
        _FUN_007a0d10 = new FhMethodHandle<FUN_007a0d10>(this, game, 0x003a0d10, h_get_weapon_name);
        // getWeaponModel
        _FUN_007a0c70 = new FhMethodHandle<FUN_007a0c70>(this, game, 0x003a0c70, h_get_weapon_model);
        // obtainTreasureCleanup
        _FUN_007993f0 = new FhMethodHandle<FUN_007993f0>(this, game, 0x003993f0, h_obtain_treasure_cleanup);


        _MsGetChr             = FhUtil.get_fptr<MsGetChr>(__addr_MsGetChr);
        _MsGetComData         = FhUtil.get_fptr<FhCall.MsGetComData>(FhCall.__addr_MsGetComData);
        _MsGetCommandUse      = FhUtil.get_fptr<FhCall.MsGetCommandUse>(FhCall.__addr_MsGetCommandUse);
        _MsGetCommandMP       = FhUtil.get_fptr<FhCall.MsGetCommandMP>(FhCall.__addr_MsGetCommandMP);
        _MsGetRamChrMonster   = FhUtil.get_fptr<FhCall.MsGetRamChrMonster>(FhCall.__addr_MsGetRamChrMonster);
        _TODrawCrossBoxXYWHC2 = FhUtil.get_fptr<FhCall.TODrawCrossBoxXYWHC2>(FhCall.__addr_TODrawCrossBoxXYWHC2);

        _eiAbmParaGet = new FhMethodHandle<eiAbmParaGet>(this, game, __addr_eiAbmParaGet, h_eiAbmParaGet);
        _MsSetSaveParam = new FhMethodHandle<MsSetSaveParam>(this, game, __addr_MsSetSaveParam, h_MsSetSaveParam);
        _MsSetRamChrParam = new FhMethodHandle<MsSetRamChrParam>(this, game, __addr_MsSetRamChrParam, h_MsSetRamChrParam);
        _FUN_00a48910 = new FhMethodHandle<FUN_00a48910>(this, game, 0x00648910, h_FUN_00a48910);

        _MsApUp = new FhMethodHandle<MsApUp>(this, game, 0x00398a10, h_MsApUp);

        //delegate* unmanaged[Thiscall]<nint, nint, bool, bool> ph_openFile = &h_openFile;
        //openFile p_openFile = (x, y, z) => d_openFile(ph_openFile, x, y, z);
        //openFile temp = Marshal.GetDelegateForFunctionPointer<openFile>((nint)ph_openFile);
        _openFile = new FhMethodHandle<openFile>(this, game, 0x00208100, h_openFile);
        _readFile = FhUtil.get_fptr<readFile>(0x00208250);

        _FUN_0070aec0 = new FhMethodHandle<FUN_0070aec0>(this, game, 0x0030aec0, h_FUN_0070aec0);


        _ChN_ReadSystemMGRP = FhUtil.get_fptr<FhCall.ChN_ReadSystemMGRP>(FhCall.__addr_ChN_ReadSystemMGRP);

        _Common_loadModel = FhUtil.get_fptr<Common_loadModel>(0x0045ce70);
        _Common_0043 = FhUtil.get_fptr<Common_0043>(0x0045c810);
        _Common_linkFieldToBattleActor = FhUtil.get_fptr<Common_linkFieldToBattleActor>(0x0045ca00);

        _SndSepPlay = FhUtil.get_fptr<FhCall.SndSepPlay>(FhCall.__addr_SndSepPlay);


        _Map_800F = new FhMethodHandle<Map_800F>(this, game, 0x0051b1a0, h_Map_800F);


        _MsGetExcelData = FhUtil.get_fptr<MsGetExcelData>(__addr_MsGetExcelData);


        // obtainTreasure related
        _TkMsImportantSet = new FhMethodHandle<TkMsImportantSet>(this, game, __addr_TkMsImportantSet, h_TkMsImportantSet);
        _MsPayGIL = FhUtil.get_fptr<MsPayGIL>(__addr_MsPayGIL); // takeGil
        _SndSepPlaySimple = FhUtil.get_fptr<SndSepPlaySimple>(__addr_SndSepPlaySimple);
        _MsGetSaveWeapon = FhUtil.get_fptr<MsGetSaveWeapon>(__addr_MsGetSaveWeapon);
        _FUN_007ab930 = FhUtil.get_fptr<FUN_007ab930>(0x003ab930); // giveWeapon
        _AtelGetCurCtrlWork = FhUtil.get_fptr<AtelGetCurCtrlWork>(__addr_AtelGetCurCtrlWork);
        _MsFieldItemGet = FhUtil.get_fptr<MsFieldItemGet>(__addr_MsFieldItemGet);
        _TkMsGetRomItem = FhUtil.get_fptr<TkMsGetRomItem>(__addr_TkMsGetRomItem);
        _MsSaveItemUse = FhUtil.get_fptr<MsSaveItemUse>(__addr_MsSaveItemUse);
        _MsImportantName = FhUtil.get_fptr<MsImportantName>(__addr_MsImportantName);
        _CT_RetInt_0065 = FhUtil.get_fptr<CT_RetInt_0065>(__addr_CT_RetInt_0065);
        _CT_RetInt_006A = FhUtil.get_fptr<CT_RetInt_006A>(__addr_CT_RetInt_006A);
        _MsBtlGetInit = FhUtil.get_fptr<FhCall.MsBtlGetInit>(FhCall.__addr_MsBtlGetInit);
        _AtelGetMesWinWork = FhUtil.get_fptr<AtelGetMesWinWork>(__addr_AtelGetMesWinWork);
        _FUN_008b8910 = FhUtil.get_fptr<FUN_008b8910>(__addr_FUN_008b8910);
        _FUN_008bda20 = FhUtil.get_fptr<FUN_008bda20>(__addr_FUN_008bda20);
        _FUN_008b8930 = FhUtil.get_fptr<FUN_008b8930>(__addr_FUN_008b8930);
        _FUN_0086a0c0 = FhUtil.get_fptr<FUN_0086a0c0>(__addr_FUN_0086a0c0);


        _FUN_0086bec0 = new FhMethodHandle<FUN_0086bec0>(this, game, 0x0046bec0, h_FUN_0086bec0);
        _FUN_0086bea0 = new FhMethodHandle<FUN_0086bea0>(this, game, 0x0046bea0, h_FUN_0086bea0);


        _FUN_00656c90 = new FhMethodHandle<FUN_00656c90>(this, game, 0x00256c90, h_FUN_00656c90);

        _TkMenuAppearMainCmdWindow = new FhMethodHandle<TkMenuAppearMainCmdWindow>(this, game, __addr_TkMenuAppearMainCmdWindow, h_TkMenuAppearMainCmdWindow);

        // For loading texture from game
        _FUN_0065ee30 = FhUtil.get_fptr<FUN_0065ee30>(__addr_ClusterManager_FUN_0065ee30);
        _ClusterManager_loadPCluster = FhUtil.get_fptr<ClusterManager_loadPCluster>(__addr_ClusterManager_loadPCluster);
        _Phyre_PFramework_PApplication_FixupClusters = FhUtil.get_fptr<Phyre_PFramework_PApplication_FixupClusters>(__addr_Phyre_PFramework_PApplication_FixupClusters);
        _ClusterManager_releasePCluster = FhUtil.get_fptr<ClusterManager_releasePCluster>(__addr_ClusterManager_releasePCluster);
        _ClusterManager_getPClusterByName = FhUtil.get_fptr<ClusterManager_getPClusterByName>(__addr_ClusterManager_getPClusterByName);
        _fiosUnifyFilename = FhUtil.get_fptr<fiosUnifyFilename>(__addr_fiosUnifyFilename);

        // Non-loading FMV
        _graphicInitFMVPlayer = new FhMethodHandle<graphicInitFMVPlayer>(this, game, __addr_graphicInitFMVPlayer, h_graphicInitFMVPlayer);

        foreach (byte[] script in customScripts) {
            customScriptHandles.Add(GCHandle.Alloc(script, GCHandleType.Pinned));
        }

        //for (int i = 0; i < rawCustomStrings.Length; i++) {
        //    byte[] text = rawCustomStrings[i];
        //    customStrings[i] = new CustomString(text);
        //}

        _FmodVoice_dataChange = new FhMethodHandle<FmodVoice_dataChange>(this, game, __addr_FmodVoice_dataChange, h_FmodVoice_dataChange);
        var _FMOD_EventSystem_load_pointer = FhUtil.get_at<nint>(__addr_FMOD_EventSystem_load);
        _FMOD_EventSystem_load = Marshal.GetDelegateForFunctionPointer<FMOD_EventSystem_load>(_FMOD_EventSystem_load_pointer);
        //_FMOD_EventSystem_load = new FhMethodHandle<FMOD_EventSystem_load>(this, _FMOD_EventSystem_load_pointer, h_FMOD_EventSystem_load);

        //_FfxFmod_soundInit_setLang = new FhMethodHandle<FfxFmod_soundInit_setLang>(this, game, __addr_FfxFmod_soundInit_setLang, h_FfxFmod_soundInit_setLang);

        _LocalizationManager_Initialize = new FhMethodHandle<LocalizationManager_Initialize>(this, game, __addr_LocalizationManager_Initialize, h_LocalizationManager_Initialize);
        _LocalizationManager_GetInstance = FhUtil.get_fptr<LocalizationManager_GetInstance>(__addr_LocalizationManager_GetInstance);
        _FfxFmod_soundInit = FhUtil.get_fptr<FfxFmod_soundInit>(__addr_FfxFmod_soundInit);
        _FmodVoice_initList = FhUtil.get_fptr<FmodVoice_initList>(__addr_FmodVoice_initList);


        // Custom namespace
        _AtelInitTotal = new FhMethodHandle<AtelInitTotal>(this, game, __addr_AtelInitTotal, h_AtelInitTotal);

        // Airship
        _FUN_00867370 = FhUtil.get_fptr<FUN_00867370>(__addr_FUN_00867370);
        _FUN_008671d0 = FhUtil.get_fptr<FUN_008671d0>(__addr_FUN_008671d0);
        _Map_show2DLayerResultInt = FhUtil.get_fptr<Map_show2DLayerResultInt>(__addr_Map_show2DLayerResultInt);
        _Map_hide2DLayerResultInt = FhUtil.get_fptr<Map_hide2DLayerResultInt>(__addr_Map_hide2DLayerResultInt);

        // Drawing
        _TOMkpCrossExtMesFontLClutTypeRGBA = FhUtil.get_fptr<TOMkpCrossExtMesFontLClutTypeRGBA>(__addr_TOMkpCrossExtMesFontLClutTypeRGBA);
        _ToMakeBtlEasyFont = FhUtil.get_fptr<ToMakeBtlEasyFont>(__addr_ToMakeBtlEasyFont);

        _PrepareMenuList = new FhMethodHandle<PrepareMenuList>(this, game, __addr_PrepareMenuList, h_PrepareMenuList);
        _UpdateCustomizationMenuState = new FhMethodHandle<UpdateCustomizationMenuState>(this, game, __addr_UpdateCustomizationMenuState, h_UpdateCustomizationMenuState);
        _DrawCustomizationMenu = new FhMethodHandle<DrawCustomizationMenu>(this, game, __addr_DrawCustomizationMenu, h_DrawCustomizationMenu);
        _MsGetRomKaizou = FhUtil.get_fptr<MsGetRomKaizou>(__addr_MsGetRomKaizou);
        _MsGetRomAbility = FhUtil.get_fptr<MsGetRomAbility>(__addr_MsGetRomAbility);

        _FUN_008c1c70                    = FhUtil.get_fptr<FUN_008c1c70>(__addr_FUN_008c1c70);
        _TODrawMenuPlateXYWHType         = FhUtil.get_fptr<TODrawMenuPlateXYWHType>(__addr_TODrawMenuPlateXYWHType);
        _FUN_008f8bb0                    = FhUtil.get_fptr<FUN_008f8bb0>(__addr_FUN_008f8bb0);
        _TODrawScissorXYWH               = FhUtil.get_fptr<TODrawScissorXYWH>(__addr_TODrawScissorXYWH);
        _FUN_008d5d20                    = FhUtil.get_fptr<FUN_008d5d20>(__addr_FUN_008d5d20);
        _FUN_008c0f40                    = FhUtil.get_fptr<FUN_008c0f40>(__addr_FUN_008c0f40);
        _FUN_008c1350_DrawScissor512x416 = FhUtil.get_fptr<FUN_008c1350_DrawScissor512x416>(__addr_FUN_008c1350_DrawScissor512x416);
        _FUN_008d5dc0                    = FhUtil.get_fptr<FUN_008d5dc0>(__addr_FUN_008d5dc0);
        _FUN_008e6cc0                    = FhUtil.get_fptr<FUN_008e6cc0>(__addr_FUN_008e6cc0);
        _FUN_008d6630                    = FhUtil.get_fptr<FUN_008d6630>(__addr_FUN_008d6630);
    }

    public static int ignore_this = 11;
    public static int h_Map_800F(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        int param_1 = atelStack->values.as_int()[0];
        if (param_1 == ignore_this) {
            atelStack->pop_int();
            return 1;
        }
        return _Map_800F.orig_fptr(work, storage, atelStack);
    }

    public static bool h_openFile(nint _this, nint filename, bool readOnly, nint unknown_1, nint unknown_2, nint unknown_3) {
        string x = Marshal.PtrToStringAnsi(filename);
        logger.Debug($"{_this}, {x}, {readOnly}, {unknown_1}, {unknown_2}, {unknown_3}");
        var result = _openFile.orig_fptr(_this, filename, readOnly, unknown_1, unknown_2, unknown_3);
        logger.Debug($"{result}, {*(int*)_this}, {*(int*)((int)_this + 4)}");
        return result;
    }

    public static uint h_FUN_0070aec0(nint _this, uint voice_id, uint param_2) {

        return _FUN_0070aec0.orig_fptr(_this, voice_id, param_2);
    }

    public bool hook() {
        return _Common_obtainTreasureInit.hook() && _Common_obtainTreasureSilentlyInit.hook() && _Common_obtainBrotherhoodRetInt.hook()
            && _TkSetLegendAbility.hook() && _Common_setPrimerCollected.hook()
            && _AtelEventSetUp.hook() && _Common_transitionToMap.hook() && _Common_warpToMap.hook()
            && _SgEvent_showModularMenuInit.hook()
            && _Common_addPartyMember.hook() && _Common_removePartyMember.hook() && _Common_removePartyMemberLongTerm.hook() && _Common_setWeaponVisibilty.hook()
            && _Common_putPartyMemberInSlot.hook() && _Common_pushParty.hook() && _Common_popParty.hook() && _MsBattleExe.hook() && _FUN_00791820.hook()
            && _MsApUp.hook() && _MsBtlReadSetScene.hook() && _MsMonsterCapture.hook() && _FUN_00783bb0.hook() //&&  // && _Map_800F.hook() //_MsBtlGetPos.hook()
            && _eiAbmParaGet.hook() && _MsSetSaveParam.hook() && _MsSetRamChrParam.hook() // && _FUN_00a48910.hook()
            && _FUN_0086bec0.hook() && _FUN_0086bea0.hook() // Custom strings
            && _graphicInitFMVPlayer.hook() && _FmodVoice_dataChange.hook()
            && _AtelInitTotal.hook()
            && _LocalizationManager_Initialize.hook()
            && _TkMenuAppearMainCmdWindow.hook()
            && _PrepareMenuList.hook() && _UpdateCustomizationMenuState.hook() && _DrawCustomizationMenu.hook();
        //&& _FUN_00656c90.hook() && _FUN_0065ee30.hook();
        //&& _openFile.hook() && _FUN_0070aec0.hook();
        //&& _MsCheckLeftWindow.hook() && _MsCheckUseCommand.hook() && _TOBtlDrawStatusLimitGauge.hook();

    }

    private static void set(byte* code_ptr, uint offset, AtelInst opcode) {
        byte* ptr = code_ptr + offset;
        foreach (byte b in opcode.to_bytes()) {
            *ptr = b;
            ptr++;
        }
    }
    private static void set(byte* code_ptr, uint[] offsets, AtelInst opcode) {
        foreach (uint offset in offsets) {
            set(code_ptr, offset, opcode);
        }
    }

    private static void set(byte* code_ptr, uint offset, AtelInst[] opcodes) {
        byte* ptr = code_ptr + offset;
        foreach (AtelInst op in opcodes) {
            foreach (byte b in op.to_bytes()) {
                *ptr = b;
                ptr++;
            }
        }
    }
    private static void set(byte* code_ptr, uint[] offsets, AtelInst[] opcodes) {
        foreach (uint offset in offsets) {
            set(code_ptr, offset, opcodes);
        }
    }



    private static byte* h_FUN_0086bec0(int param_1) {
        byte* result;
        if ((param_1 & 0x8000) != 0) {
            int custom_index = param_1 & 0x7FFF;
            result = customStrings[custom_index].encoded;
            //result = (byte*)customStringHandles[custom_index].AddrOfPinnedObject();
            //FhEncoding.compute_decode_buffer_size(result);
            //string decoded = FhEncoding.Us.to_string(result);
            logger.Debug(customStrings[custom_index].decoded);
        } else {
            // May crash if called with invalid index
            result = _FUN_0086bec0.orig_fptr(param_1);
        }

        return result;
    }
    private static short h_FUN_0086bea0(int param_1) {
        short result;
        if ((param_1 & 0x8000) != 0) {
            int custom_index = param_1 & 0x7FFF;
            result = customStrings[custom_index].metadata;
        }
        else {
            result = _FUN_0086bea0.orig_fptr(param_1);
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

    private static readonly List<GCHandle> customScriptHandles = [];
    private static readonly byte[][] customScripts = {
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

    //TODO: Figure out how to split & handle the multiple NPC's in one room.
    private static readonly Dictionary<string, (uint offset, ushort recruit_id)[]> event_to_recruit_offsets = new(){
        {"bsvr0400", [(0x25EC, 19)] }, // Vilucha
        {"djyt0000", [(0x400B,  6)] }, // Kyou
        {"genk1100", [(0x13D0, 10)] }, // Miyu
        {"guad0300", [(0x20FA, 22)] }, // Yuma Guado
        {"hiku0000", [(0x5906, 13)] }, // Rin
        {"hiku0500", [(0x7499, 13)] }, // Rin
        {"hiku0800", [(0x57EC, 20), (0xCAA0,  2)] }, // Wakka & Brother
        {"hiku0801", [(0x34DB, 20), (0x687F,  2)] }, // Wakka & Brother
        {"hiku1900", [(0x23C4, 20), (0x6843,  2)] }, // Wakka & Brother
        {"kami0100", [(0x9A8F,  9)] }, // Mifurey
        {"klyt0600", [(0x12C4,  8)] }, // Mep
        {"lchb0000", [(0x1C46,  1), (0x34F8,  21)] }, // Biggs & Wedge
        {"lchb0100", [(0x41A6, 12)] }, // Nedus
        {"lchb0500", [(0x35EC, 24)] }, // Zev Ronso
        {"lchb0900", [(0x1F58, 23)] }, // Zalitz
        {"lchb1800", [(0x4407, 15)] }, // Shaami
        {"luca0100", [(0x6DB9,  4)] }, // Jumal
        {"luca0400", [(0x4402, 16)] }, // Shuu
        {"mcyt0000", [(0x3B7D,  7)] }, // Linna
        {"mihn0300", [(0x8A4A, 14)] }, // Ropp
        {"nagi0000", [(0x8872, 17), (0x33DDD, 11)] }, // Svanda & Naida
        {"nagi0400", [(0x2711,  3)] }, // Durren
        {"ptkl0200", [(0x6538, 18)] }, // Tatts
        {"ptkl0600", [(0x2AC1, 18)] }, // Tatts
        {"swin0000", [(0x8981,  5)] }, // Kiyuri
    };

    private static readonly Dictionary<string, (uint offset, ushort southNorth)[]> event_to_lightning_dodging_offsets = new(){
        {"kami0000", [(0x3848, 0)] },
        {"kami0300", [(0x685F, 1)] },
    };

    private static Dictionary<(int, int), uint> originalEntryPoints = new();
    private static string current_event_name = "";
    private static void h_AtelEventSetUp(int event_id) {
        _AtelEventSetUp.orig_fptr(event_id);

        foreach (NativeCustomString customString in cached_strings) {
            customString.Free();
        }
        cached_strings.Clear();

        originalEntryPoints.Clear();

        foreach ((string key, CustomStringDrawInfo drawInfo) in customStringDrawInfos) {
            if (!drawInfo.persistent) customStringDrawInfos.Remove(key);
        }

        string event_name = Marshal.PtrToStringAnsi((nint)get_event_name((uint)event_id))!;
        logger.Debug($"atel_event_setup: {event_name}");
        byte* code_ptr = Atel.controllers[0].worker(0)->code_ptr;
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
            case "hiku2100":
                logger.Debug($"atel_event_setup: Inject set_airship_destinations call");
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

                foreach ((RegionEnum region, ArchipelagoRegion state) in region_states) {
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
                logger.Debug($"atel_event_setup: Inject Cid talk hook");
                set(code_ptr, 0x4DC5, [
                    AtelOp.PUSHII  .build(0x0000),
                    AtelOp.CALLPOPA.build((ushort)CustomCallTarget.JUMP), // Common.Jump(0002) = jump to customScripts[0]
                ]);
                break;
            case "hiku0500":
                // Rin collected primers check inventory instead of count
                set(code_ptr, 0x5869, AtelOp.CALL.build((ushort)CustomCallTarget.COLLECTED_PRIMERS));
                break;
            case "ssbt0300":
                logger.Debug($"atel_event_setup: Redirect Overdrive Sin post-battle warp");
                set(code_ptr, 0x500E, AtelOp.PUSHII.build(382));
                break;
            case "sins0700":
                logger.Debug($"atel_event_setup: Handle removing Aeons");
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
                logger.Debug($"atel_event_setup: Wait longer");
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
                // Tower falling down
                Globals.Atel.current_controller->worker(0xD)->table_jump[0] = 0x1951;
                AtelBasicWorker* _0xd = Globals.Atel.current_controller->worker(0xD);
                set(code_ptr, 0x1947, [
                    AtelOp.PUSHII  .build(0x0003),
                    AtelOp.CALLPOPA.build((ushort)CustomCallTarget.JUMP), // Common.Jump(3) = jump to customScripts[3]
                    ]);

                // Look up at tower
                //set(code_ptr, 0x1761, [
                //    AtelOp.RET.build(),
                //    ]);
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

                break;
            case "nagi0600":
                // Send Yojimbo
                set(code_ptr, [0x261A, 0x2686, 0x28FB, 0x2B20, 0x2D45, 0x2F39], [
                    AtelOp.PUSHII  .build(14),
                    AtelOp.CALLPOPA.build((ushort)CustomCallTarget.SEND_PARTY_MEMBER_LOCATION),
                    ]);
                break;
            case "nagi0700":
                // Turn around if locked
                //set(code_ptr, 0x13AAF + 6, [
                //    AtelOp.CALLPOPA     .build((ushort)CustomCallTarget.BLOCK_WARP),
                //    ]);
                //set(code_ptr, 0x13A92, atelNOPArray(17));

                // Always show Nirvana chest
                set(code_ptr, 0xE25D, AtelOp.JMP.build(0));

                // Continue even if Calm Lands conquest not complete
                set(code_ptr, 0xE07B, AtelOp.JMP.build(2));

                // Skip introduction and explanation
                set(code_ptr, 0xDCF3, AtelOp.JMP.build(0xB29));

                // Unlock Monster Arena
                save_data->event_flags[0].set_bit(0, true);

                save_data->monsters_captured[43] = 99;
                save_data->monsters_captured[59] = 99;


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
            logger.Info($"Save sphere init at {save_sphere_offset}");
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
            logger.Info($"Underwater save sphere init at {save_sphere_offset}");
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
            logger.Info($"Underwater save sphere init at {save_sphere_offset}");
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
            logger.Info($"Underwater save sphere init at {save_sphere_offset}");
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
            logger.Info($"Save sphere init at {save_sphere_offset}");
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
            logger.Info($"Save sphere init at {save_sphere_offset}");
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
            logger.Debug($"{save_sphere_worker_1->table_var[1].raw}");
            save_sphere_worker_1->table_var[1].raw = 0x0000000100000A98;

            AtelBasicWorker* save_sphere_worker_2 = Globals.Atel.current_controller->worker(0x14);
            // Custom switch
            save_sphere_worker_2->table_jump[1] = (uint)(customScriptHandles[6].AddrOfPinnedObject() - (nint)save_sphere_worker_2->code_ptr);
        }
        else {
            AtelInst? previous_op = null;
            AtelInst? current_op = null;
            uint code_length = Atel.controllers[0].worker(0)->script_chunk->code_length;
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
                        logger.Info("Potentially incorrect Save Sphere offsets");
                    }
                    save_spheres_detected++;
                    logger.Info($"Detected save sphere init at {i - 6}");
                    uint save_sphere_offset = i - 6;

                    AtelOp someInst = (AtelOp)code_ptr[save_sphere_offset + sphere_level_offset];
                    if (!someInst.has_operand() || someInst.build(*(ushort*)(code_ptr + save_sphere_offset + sphere_level_offset + 1)) != AtelOp.PUSHV.build(0x0000)) {
                        logger.Warning($"Unexpected instruction at {save_sphere_offset + sphere_level_offset}");
                    }
                    set(code_ptr, save_sphere_offset + sphere_level_offset, AtelOp.JMP.build(0x0007)); // Always all options



                    someInst = (AtelOp)code_ptr[save_sphere_offset + tutorial_offset + 13];
                    if (!someInst.has_operand() || someInst.build(*(ushort*)(code_ptr + save_sphere_offset + tutorial_offset + 13 + 1)) != AtelOp.POPXNCJMP.build(tutorial_jump)) {
                        logger.Warning($"Unexpected instruction at {save_sphere_offset + tutorial_offset + 13}");
                    }
                    // Update region state. Also skips save sphere tutorial
                    set(code_ptr, save_sphere_offset + tutorial_offset, [
                        AtelOp.CALLPOPA.build((ushort)CustomCallTarget.UPDATE_REGION_STATE),
                        AtelOp.JMP     .build(tutorial_jump),
                        ]);


                    someInst = (AtelOp)code_ptr[save_sphere_offset + airship_warp_offset];
                    if (!someInst.has_operand() || someInst.build(*(ushort*)(code_ptr + save_sphere_offset + airship_warp_offset + 1)) != AtelOp.PUSHV.build(0x0000)) {
                        logger.Warning($"Unexpected instruction at {save_sphere_offset + airship_warp_offset}");
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

    private static void h_Common_obtainTreasureInit(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        int treasure_id = atelStack->values.as_int()[1];
        logger.Info($"obtain_treasure: {treasure_id}");
        //_Common_obtainTreasureInit.orig_fptr(work, storage, atelStack);
        obtainTreasureInitReimplement(work, storage, atelStack);
    }


    private static void obtainTreasureInitReimplement(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        byte* item_name = (byte*)0;

        int treasure_id = atelStack->pop_int();
        int window_id = atelStack->pop_int();
        logger.Debug($"window_id:{window_id}, treasure_id:{treasure_id}");

        _SndSepPlaySimple(0x80000026);
        AtelWorkerController* pAVar2 = (AtelWorkerController*)_AtelGetCurCtrlWork();
        ((byte*)pAVar2)[3] |= 4;
        _MsFieldItemGet(treasure_id);
        _FUN_008b8910(window_id, 0, 0);
        _FUN_008b8910(window_id, 1, 1);
        bool gear_inv_is_full = false;
        uint weapon_id = 0;
        byte* message_text = _FUN_008bda20(0x401d); // "Nothing"


        if (item_locations.treasure.TryGetValue(treasure_id, out var item)) {
            if (FFXArchipelagoClient.sendLocation(treasure_id, ArchipelagoLocationType.Treasure)) {
                obtain_item(item.id);

                NativeCustomString name = new NativeCustomString(item.id != 0 ? item.name : $"{item.name} to {item.player}", encodingFlags: FhEncodingFlags.IGNORE_EXPRESSIONS);
                //CustomString name = new CustomString(item.name, encodingFlags: FhEncodingFlags.IGNORE_EXPRESSIONS);
                logger.Info(item.name);

                cached_strings.Add(name);
                item_name = name.encoded;
                if (item.id != 0) {
                    message_text = _FUN_008bda20(0x4018); // "Obtained %0!"
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
            _TkMsGetRomItem(Battle.reward_data->items[0], (int*)&item_name);
            _FUN_008b8930(window_id, 1, Battle.reward_data->items_amounts[0]);
            if (Battle.reward_data->items_amounts[0] == 1) {
                message_text = _FUN_008bda20(0x4018); // "Obtained %0!"
            }
            else {
                message_text = _FUN_008bda20(0x4019); // "Obtained %0 x%1!"
            }

            byte[] decoded = new byte[FhEncoding.compute_decode_buffer_size(new ReadOnlySpan<byte>(item_name, 1000))];

            int decoded_length = FhEncoding.decode(new ReadOnlySpan<byte>(item_name, 1000), decoded, flags:FhEncodingFlags.IMPLICIT_END);
            //string decoded = FhEncoding.Us.to_string(item_name);
            logger.Info(Encoding.UTF8.GetString(decoded, 0, decoded_length));

            _MsSaveItemUse(Battle.reward_data->items[0], Battle.reward_data->items_amounts[0]);
        }
        else if (Battle.reward_data->key_item_count != 0) {
            item_name = _MsImportantName(Battle.reward_data->key_item);
            message_text = _FUN_008bda20(0x4017); // "Obtained %0!"
            _TkMsImportantSet.hook_fptr(Battle.reward_data->key_item);
        }
        else if (Battle.reward_data->gear_count != 0) {
            weapon_id = Battle.reward_data->gear_inv_idx;
            message_text = _FUN_008bda20(0x4016); // "Obtained %0!"
            Equipment* weapon = (Equipment*)_MsGetSaveWeapon(weapon_id, (nint)(&item_name));
            int inv_id = _FUN_007ab930(weapon); // giveWeapon?
            gear_inv_is_full = inv_id == 0;
        }
        else if (Battle.reward_data->gil != 0) {
            _FUN_008b8930(window_id, 1, (int)Battle.reward_data->gil);
            message_text = _FUN_008bda20(0x401a); // "Obtained %1 Gil!"
            _MsPayGIL(-(int)Battle.reward_data->gil);
        }

        _FUN_008b8930(window_id, 0, (int)item_name);

        atelStack->push_int(window_id);
        atelStack->push_int(0x100);
        atelStack->push_int(0xd0);
        atelStack->push_int(4);
        _CT_RetInt_0065((nint)work, storage, (nint)atelStack);

        TOMesWinWork* mesageWindowWorker = _AtelGetMesWinWork(window_id);
        mesageWindowWorker->_0x20 = 0;
        mesageWindowWorker->text = message_text;
        mesageWindowWorker->_0xc = message_text;
        mesageWindowWorker->_0x16 = 0;
        mesageWindowWorker->_0x18 = 0;

        atelStack->push_int(window_id);
        atelStack->push_int(0);
        _CT_RetInt_006A((nint)work, storage, (nint)atelStack);

        *storage = window_id;
        storage[1] = 1;
        storage[2] = gear_inv_is_full ? 1 : 0;
        storage[3] = (int)weapon_id;

        if (!gear_inv_is_full) {
            _MsBtlGetInit();
        }
        _FUN_0086a0c0();
        mesageWindowWorker->_0x1d |= 0x10;
    }

    private static void h_Common_obtainTreasureSilentlyInit(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        int treasure_id = atelStack->values.as_int()[0];
        logger.Info($"obtain_treasure_silently: {treasure_id}");
        //_Common_obtainTreasureSilentlyInit.orig_fptr(work, storage, atelStack);
        obtainTreasureSilentlyInitReimplement(work, storage, atelStack);
    }

    private static void obtainTreasureSilentlyInitReimplement(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {

        int treasure_id = atelStack->pop_int();
        _MsFieldItemGet(treasure_id);
        bool gear_inv_is_full = false;
        uint weapon_id = 0;

        if (item_locations.treasure.TryGetValue(treasure_id, out var item)) {
            if (FFXArchipelagoClient.sendLocation(treasure_id, ArchipelagoLocationType.Treasure)) {
                obtain_item(item.id);
            }
        }
        else
        if (Battle.reward_data->item_count != 0) {
            _MsSaveItemUse(Battle.reward_data->items[0], Battle.reward_data->items_amounts[0]);
        }
        else if (Battle.reward_data->key_item_count != 0) {
            _TkMsImportantSet.hook_fptr(Battle.reward_data->key_item);
        }
        else if (Battle.reward_data->gear_count != 0) {
            weapon_id = Battle.reward_data->gear_inv_idx;
            Equipment* weapon = (Equipment*)_MsGetSaveWeapon(weapon_id, 0);
            int inv_id = _FUN_007ab930(weapon); // giveWeapon?
            gear_inv_is_full = inv_id == 0;
        }
        else if (Battle.reward_data->gil != 0) {
            _MsPayGIL(-(int)Battle.reward_data->gil);
        }

        storage[2] = gear_inv_is_full ? 1 : 0;
        storage[3] = (int)weapon_id;

        if (!gear_inv_is_full) {
            _MsBtlGetInit();
        }
    }

    private static int h_Common_obtainBrotherhoodRetInt(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        logger.Debug($"obtain_brotherhoodRetInt");
        if (atelStack->size > 0) {
            throw new Exception($"Too many parameters ({atelStack->size}) passed to obtainBrotherhood!");
        }
        if (item_locations.other.TryGetValue(0, out var item)) {
            obtain_item(item.id);
            return 1;
        }
        return _Common_obtainBrotherhoodRetInt.orig_fptr(work, storage, atelStack);
    }
    private static int h_Common_upgradeBrotherhoodRetInt(nint work, int* storage, nint atelStack) {
        logger.Debug($"upgrade_brotherhoodRetInt");
        if (item_locations.other.TryGetValue(37, out var item)) {
            obtain_item(item.id);
            return 1;
        }

        return _Common_upgradeBrotherhoodRetInt.orig_fptr(work, storage, atelStack);
    }
    private static int h_Common_isBrotherhoodUnpoweredRetInt(nint work, int* storage, nint atelStack) {
        logger.Debug($"isBrotherhoodUnpoweredRetInt");

        return _Common_isBrotherhoodUnpoweredRetInt.orig_fptr(work, storage, atelStack);
    }
    //private static int h_Common_grantCelestialUpgrade(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
    //    int character = atelStack->values.as_int()[0];
    //    int level = atelStack->values.as_int()[1];
    //    logger.Debug($"grant_celestial_upgrade: character={id_to_character[character]}, level={level}");
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

    private static int h_TkSetLegendAbility(int chr_id, int level) {
        logger.Debug($"grant_celestial_upgrade: character={id_to_character[chr_id]}, level={level}");

        int other_id = 37 + level + chr_id * 2;
        if (0 <= chr_id && chr_id <= 6 && item_locations.other.TryGetValue(other_id, out var item)) {
            if (FFXArchipelagoClient.sendLocation(other_id, ArchipelagoLocationType.Other)) {
                obtain_item(item.id);
            }
            return level;
        }

        return _TkSetLegendAbility.orig_fptr(chr_id, level);
    }


    private static int h_Common_setPrimerCollected(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        int primer = atelStack->values.as_int()[0];
        logger.Debug($"set_primer_collected: Al Bhed Primer {primer + 1}");

        if (item_locations.other.TryGetValue(primer + 1, out var item)) {
            if (FFXArchipelagoClient.sendLocation(primer + 1, ArchipelagoLocationType.Other)) {
                obtain_item(item.id);
            }
            atelStack->pop_int();
            return 1;
        }

        return _Common_setPrimerCollected.orig_fptr(work, storage, atelStack);
    }

    private static void update_region_state() {
        update_region_state(save_data->current_room_id, save_data->current_spawnpoint);
    }
    private static void update_region_state(int map, int entrance) {
        logger.Info($"Update region state");

        // Update region state
        if (current_region != RegionEnum.None && region_states.TryGetValue(current_region, out ArchipelagoData.ArchipelagoRegion? current_state)) {
            current_state = region_states[current_region];

            for (int i = 0; i < current_state.savedata.Length; i++) {
                ref var data = ref current_state.savedata[i];
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

    private static void restore_region_state(ref int map, ref int entrance) {
        ArchipelagoRegion current_state = region_states[current_region];
        save_data->story_progress = current_state.story_progress;
        map      = current_state.room_id;
        entrance = (byte)current_state.entrance;

        foreach (var data in current_state.savedata) {
            data.bytes.CopyTo(new Span<byte>((byte*)((int)save_data + data.offset), data.size));
        }
    }

    private static void on_map_change() {
        int map      = save_data->current_room_id;
        int entrance = save_data->current_spawnpoint;
        handle_warp_transition(save_data->last_room_id, save_data->last_spawnpoint, ref map, ref entrance); // We can ignore return value?
        if (map == 382) update_region_state(save_data->last_room_id, save_data->last_spawnpoint);
        if (save_data->current_room_id != map) {
            save_data->current_room_id    = (ushort)map;
            save_data->current_spawnpoint = (byte)entrance;
        }
    }
    private static bool handle_warp_transition(int current_map, int current_entrance, ref int next_map, ref int next_entrance) {
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
                FFXArchipelagoClient.local_checked_locations.Clear();
                FFXArchipelagoClient.received_items = 0;
                FFXArchipelagoClient.remote_locations_updated = true;
                // Load seed here?
                if (!loadSeed()) {
                    next_map = 23;
                    next_entrance = 0;
                    return handle_warp_transition(current_map, current_entrance, ref next_map, ref next_entrance);
                    //return true;
                }
                foreach (uint item in seed.StartingItems) obtain_item(item);
            }
            if (seed.SeedId is null) {
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
            else {
                // Skip crystal collecting
                if (next_map == 324 && save_data->story_progress == 3250) {
                    logger.Info($"Skipping crystal collecting");
                    next_map = 325;
                    next_entrance = 0;
                    save_data->story_progress = 3260;
                    return handle_warp_transition(current_map, current_entrance, ref next_map, ref next_entrance);
                }
            }
        }
        else {
            if (next_map == 23) {
                logger.Debug("Enter main menu");
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

    private static void refill_spheres() {
        uint[] spheres = {
            0x2046, // Power Sphere
            0x2047, // Mana Sphere
            0x2048, // Speed Sphere
            0x2049  // Ability Sphere
        };

        foreach (var item_id in spheres) {
            uint inventory_amount = save_data->get_item_count((int)item_id);
            if (inventory_amount < 40) {
                h_give_item(item_id, 40 - (int)inventory_amount);
            }
        }
    }

    private static void refill_inventory() {
        logger.Debug($"Refill inventory");

        refill_spheres();

        foreach ((var item_id, var amount) in excess_inventory) {
            if (amount > 0 && save_data->get_item_count((int)item_id) < 99) {
                excess_inventory[item_id] = 0;
                h_give_item(item_id, amount);
            }
        }
    }

    private static int h_Common_transitionToMap(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        ref int map      = ref atelStack->values.as_int()[0];
        ref int entrance = ref atelStack->values.as_int()[1];
        logger.Debug($"transition_to_map: map={map}, entrance={entrance}");

        if (handle_warp_transition(save_data->current_room_id, save_data->current_spawnpoint, ref map, ref entrance)) {
            return _Common_transitionToMap.orig_fptr(work, storage, atelStack);
        }
        else {
            return blockWarp(work, storage, atelStack);
        }
    }
    private static int h_Common_warpToMap(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        ref int map      = ref atelStack->values.as_int()[0];
        ref int entrance = ref atelStack->values.as_int()[1];
        logger.Debug($"warp_to_map: map={map}, entrance={entrance}");

        // Skip intro
        if (map == 348) {
            logger.Debug("Skip intro");
            map = 23;
        }

        if (handle_warp_transition(save_data->current_room_id, save_data->current_spawnpoint, ref map, ref entrance)) {
            return _Common_warpToMap.orig_fptr(work, storage, atelStack);
        } else {
            return blockWarp(work, storage, atelStack);
        }
    }

    private static void h_SgEvent_showModularMenuInit(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
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
            logger.Debug($"Skipping menu: type={menuTypeString}, index={index} {(unknown1 == 0x40 ? "" : $", Unknown1={unknown1}")} {(unknown2 == 0x00 ? "" : $", Unknown2={unknown2}")}");
            //FhUtil.set_at(0x00efbbf0, 0x40080000);
            //FhUtil.set_at(0x00efbbf4, 0xffffffff);
            atelStack->pop_int();
            FhUtil.set_at(0x01efb4d4, 0); // Don't wait for menu
            return;
        }

        if (menuType == 0x80) {
            logger.Info($"Unknown tutorial?");
        }

        logger.Info($"Opening menu: type={menuTypeString}, index={index} {(unknown1 == 0x40 ? "" : $", Unknown1={unknown1}")} {(unknown2 == 0x00 ? "" : $", Unknown2={unknown2}")}");
        _SgEvent_showModularMenuInit.orig_fptr(work, storage, atelStack);
    }


    public static int h_Common_addPartyMember(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        int character = atelStack->values.as_int()[0];

        //if (!party_overridden && !(save_data->atel_is_push_member == 1)) {
        if (!party_overridden) {
            if (!is_character_unlocked(character)) {
                atelStack->pop_int();
                return 1;
            }
        }
        //logger.Debug($"add_party_member: character={id_to_character[character]}");

        return _Common_addPartyMember.orig_fptr(work, storage, atelStack);
    }
    public static int h_Common_removePartyMember(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        int character = atelStack->values.as_int()[0];

        //if (!party_overridden && !(save_data->atel_is_push_member == 1)) {
        if (!party_overridden) {
            if (is_character_unlocked(character)) {
                atelStack->pop_int();
                return 1;
            }
        }
        //logger.Debug($"remove_party_member: character={id_to_character[character]}");

        return _Common_removePartyMember.orig_fptr(work, storage, atelStack);
    }
    public static int h_Common_removePartyMemberLongTerm(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        int character = atelStack->values.as_int()[0];

        //if (!party_overridden && !(save_data->atel_is_push_member == 1)) {
        if (!party_overridden) {
            if (is_character_unlocked(character)) {
                atelStack->pop_int();
                return 1;
            }
        }
        //logger.Debug($"remove_party_member_long_term: character={id_to_character[character]}");

        return _Common_removePartyMemberLongTerm.orig_fptr(work, storage, atelStack);
    }
    public static int h_Common_setWeaponInvisible(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        int character = (byte)atelStack->values.as_int()[0];
        int state = atelStack->values.as_int()[1];
        logger.Debug($"character={id_to_character[character]}, state={state}");
        if (state != 0 && is_character_unlocked(character)) {
            atelStack->pop_int();
            atelStack->pop_int();
            return 0;
        }
        return _Common_setWeaponVisibilty.orig_fptr(work, storage, atelStack);
    }
    public static int h_Common_putPartyMemberInSlot(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
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

        return _Common_putPartyMemberInSlot.orig_fptr(work, storage, atelStack);
    }
    public static int h_Common_pushParty(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        //logger.Debug($"push_party");

        //if (party_overridden) return _Common_pushParty.orig_fptr(work, storage, atelStack);
        //save_party();
        return 0;
    }
    public static int h_Common_popParty(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        //logger.Debug($"pop_party");

        //if (party_overridden) return _Common_popParty.orig_fptr(work, storage, atelStack);
        //reset_party();
        return 1;
    }

    // TODO: Get function pointer instead of hook for most of these
    public static void h_MsGetSavePartyMember(uint* param_1, uint* param_2, uint* param_3) {
        //logger.Debug($"get_current_party_slots");
        _MsGetSavePartyMember.orig_fptr(param_1, param_2, param_3);

        //logger.Debug($"get_current_party_slots: slot_0={*param_1}, slot_1={*param_2}, slot_2={*param_3}");
    }

    // Pre-battle
    public static void h_MsBattleExe(uint param_1, int field_idx, int group_idx, int formation_idx) {
        var field_ptr = Battle.btl->ptr_btl_bin_fields + field_idx * 0xe;
        string field_name = Marshal.PtrToStringAnsi((nint)(field_ptr+6));

        var group_ptr = _MsBtlListGroup(field_idx, group_idx);
        byte group_name = *(byte*)(group_ptr+5 + formation_idx * 2);

        string encounter_name = $"{field_name}_{group_name:00}";
        logger.Debug($"{encounter_name}: param_1={param_1}");
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
        if (encounterToActionDict.TryGetValue(encounter_name, out Action? action)) {
            action();
        }
        else {
            save_party(); // Probably?
            reset_party();
        }
        _MsBattleExe.orig_fptr(param_1, field_idx, group_idx, formation_idx);
    }

    public static bool h_MsMonsterCapture(int target_id, int arena_idx) {
        bool captured = _MsMonsterCapture.orig_fptr(target_id, arena_idx);

        logger.Info($"Fiend Capture: Target={target_id}, Arena Index={arena_idx}, Captured={captured}");

        // Send AP Location if successfully captured
        if (captured) {
            if (sendLocation(arena_idx, ArchipelagoLocationType.Capture) && item_locations.capture.TryGetValue(arena_idx, out var item)) {
                ArchipelagoFFXModule.obtain_item(item.id);
            }

            int qty = save_data->monsters_captured[arena_idx];
            if (qty > 0)
                FFXArchipelagoClient.current_session?.DataStorage[Scope.Slot, "FFX_CAPTURE_" + arena_idx] = qty;
            else
                FFXArchipelagoClient.current_session?.DataStorage[Scope.Slot, "FFX_CAPTURE_" + arena_idx] = 0;
        }
        return captured;
    }

    private static HashSet<ushort> initialized_monsters = [];
    public static void h_FUN_00783bb0(byte mon_id) {
        byte num_initialized = FhUtil.get_at<byte>(0xD2CA80);
        if (num_initialized == 0) initialized_monsters.Clear();

        _FUN_00783bb0.orig_fptr(mon_id);

        Chr* mon = _MsGetMon(mon_id);
        if (initialized_monsters.Add(mon->chr_id)) {
            MonStats* stats = (MonStats*)mon->ptr_base_stats;
            //logger.Debug($"{mon->chr_id & 0xFFF} stats:  stats=\n{stats->ToString()}");
            if (
                (mon->chr_id & 0xFFF) == 041 ||  // Sahagin
                (mon->chr_id & 0xFFF) == 042 ||  // Sahagin
                (mon->chr_id & 0xFFF) == 043 ||  // Sahagin
                (mon->chr_id & 0xFFF) == 068 ||  // Piranha
                (mon->chr_id & 0xFFF) == 069 ||  // Piranha
                (mon->chr_id & 0xFFF) == 070 ||  // Piranha   
                (mon->chr_id & 0xFFF) == 101 ||  // Tros
                (mon->chr_id & 0xFFF) == 155 ||  // Sahagin
                (mon->chr_id & 0xFFF) == 156 ||  // Sahagin Chief
                (mon->chr_id & 0xFFF) == 157 ||  // Garuda
                (mon->chr_id & 0xFFF) == 230 ||  // Garuda
                (mon->chr_id & 0xFFF) == 231 ||  // Dingo
                (mon->chr_id & 0xFFF) == 232 ||  // Water Flan
                (mon->chr_id & 0xFFF) == 233 ||  // Condor
                (mon->chr_id & 0xFFF) == 234     // Ragora - Lancet Tutorial
               )
                stats->monster_arena_idx = 0xFF;

            //ChrLoot* loot = (ChrLoot*)(((int*)mon->ptr_mon_wep_bin)[5] + mon->ptr_mon_wep_bin);
            //
            //loot->drop_chance_equipment = 255;
            //loot->equipment_loot.ability_count = 100; // Guaranteed 4 abilities?
            //loot->equipment_loot.slot_count = 20; // Guaranteed 4 slots
            //
            //for (int chr = 0; chr < 7; chr++) {
            //    // Guaranteed Capture
            //    loot->equipment_loot.abilities_tidus.weapon_abilities[0] = 0x807A;
            //    loot->equipment_loot.abilities_yuna.weapon_abilities[0] = 0x807A;
            //    loot->equipment_loot.abilities_auron.weapon_abilities[0] = 0x807A;
            //    loot->equipment_loot.abilities_kimahri.weapon_abilities[0] = 0x807A;
            //    loot->equipment_loot.abilities_wakka.weapon_abilities[0] = 0x807A;
            //    loot->equipment_loot.abilities_lulu.weapon_abilities[0] = 0x807A;
            //    loot->equipment_loot.abilities_rikku.weapon_abilities[0] = 0x807A;
            //    for (int i = 1; i < 8; i++) {
            //        loot->equipment_loot.abilities_tidus.weapon_abilities[i] = (ushort)(0x8000 + rng.Next(0x81));
            //        loot->equipment_loot.abilities_yuna.weapon_abilities[i] = (ushort)(0x8000 + rng.Next(0x81));
            //        loot->equipment_loot.abilities_auron.weapon_abilities[i] = (ushort)(0x8000 + rng.Next(0x81));
            //        loot->equipment_loot.abilities_kimahri.weapon_abilities[i] = (ushort)(0x8000 + rng.Next(0x81));
            //        loot->equipment_loot.abilities_wakka.weapon_abilities[i] = (ushort)(0x8000 + rng.Next(0x81));
            //        loot->equipment_loot.abilities_lulu.weapon_abilities[i] = (ushort)(0x8000 + rng.Next(0x81));
            //        loot->equipment_loot.abilities_rikku.weapon_abilities[i] = (ushort)(0x8000 + rng.Next(0x81));
            //    }
            //}

        } else {
            logger.Debug($"{mon_id}: already initialized ({mon->chr_id})");
        }
    }

    // Battle loop?
    public static void h_FUN_00791820() {
        _FUN_00791820.orig_fptr();
        string encounter_name = Marshal.PtrToStringAnsi((nint)(&Battle.btl->field_name));
        byte battle_end_type = Battle.btl->battle_end_type;
        byte battle_state = Battle.btl->battle_state;

        if (battle_end_type > 1 && battle_state == 0x21) {
            if (party_overridden) {
                reset_party();
                // Battle frontline gets copied after this so have to set here
                for (int i = 0; i < 3; i++) {
                    Battle.btl->__0x1FC5[i] = save_data->party_order[i];
                }
                for (int i = 0; i < 4; i++) {
                    Battle.btl->__0x1FD3[i] = save_data->party_order[i + 3];
                }
                party_overridden = false;
            }
            switch (battle_end_type) {
                case 2: // Battle Victory
                    logger.Info($"Victory: type={battle_end_type}, encounter={encounter_name}");
                    if (encounterVictoryActions.TryGetValue(encounter_name!, out Action? victoryAction)) {
                        victoryAction();
                    }
                    if (encounterToLocationDict.TryGetValue(encounter_name!, out int[]? boss_locations)) {
                        foreach (int location_id in boss_locations) {
                            // Sending all locations even if they don't exist
                            if (sendLocation(location_id, ArchipelagoLocationType.Boss) && item_locations.boss.TryGetValue(location_id, out var item)) {
                                ArchipelagoFFXModule.obtain_item(item.id);
                            }
                        }
                    }
                    break;
                case 3: // Battle Escape
                    logger.Info($"Escape: type={battle_end_type}, encounter={encounter_name}");
                    if (encounterEscapeActions.TryGetValue(encounter_name!, out Action? escapeAction)) {
                        escapeAction();
                    }
                    break;
                default:
                    logger.Info($"Battle End: type={battle_end_type}, encounter={encounter_name}");
                    break;
            }
        }
    }

    private static void* curr_pos_area_ptr = null;
    public static byte h_MsBtlReadSetScene() {
        byte result = _MsBtlReadSetScene.orig_fptr();
        logger.Info($"Battle Name: {Marshal.PtrToStringAnsi((nint)FhUtil.ptr_at<char>(0xD2C25A))}");
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

    public static int h_MsBtlGetPos(int param_1, Chr* chr, int btl_pos_a, int btl_pos_b, int btl_pos_c, Vector4* out_pos) {
        // btl_pos_a: position group/set/area
        // btl_pos_b = 3: party_pos[btl_pos_c]
        // btl_pos_b = 4: aeon_pos[btl_pos_c]
        // btl_pos_b = 5: enemy_pos[btl_pos_c]
        // btl_pos_b = 6: party_run_pos[btl_pos_c]?
        // btl_pos_b = 7: aeon_run_pos[btl_pos_c]?
        // btl_pos_b = 8: enemy_run_pos[btl_pos_c]?
        // btl_pos_b = 9: b?
        // btl_pos_b = 10: c?
        return _MsBtlGetPos.orig_fptr(param_1, chr, btl_pos_a, btl_pos_b, btl_pos_c, out_pos);
    }

    private static uint h_give_item(uint item_id, int amount) {
        logger.Debug($"give_item: {amount} x {item_id}");

        int new_amount = (int)(save_data->get_item_count((int)item_id) + amount);
        if (new_amount > 99) {
            int excess = new_amount - 99;
            int old_excess = excess_inventory.GetValueOrDefault(item_id, 0);
            excess_inventory[item_id] = excess + old_excess;
            amount -= excess;
        }

        return _FUN_007905a0.orig_fptr(item_id, amount);
    }
    private static void h_TkMsImportantSet(uint param_1) {
        logger.Debug($"give_key_item: {param_1}");

        _TkMsImportantSet.orig_fptr(param_1);
    }
    private static byte* h_read_from_bin(int param_1, short* param_2, int param_3) {
        logger.Debug($"get_from_bin: {param_1}, {(int)param_2}, {param_3}");
        logger.Debug($"bin_pointers: {(uint)param_2}, {(uint)(*takara_pointer)}, {(uint)(*buki_get_pointer)}");
        if (param_2 == (short*)*takara_pointer) {
            logger.Debug($"get_from_bin: takara.bin");
        }
        else if (param_2 == (short*)*buki_get_pointer) {
            logger.Debug($"get_from_bin: buki_get.bin");
        }
        var result = _FUN_007ab890.orig_fptr(param_1, param_2, param_3);
        logger.Debug($"get_from_bin: result = {((uint)result).ToString("X")}");
        return result;
    }
    private static ushort h_get_weapon_name(Equipment* param_1) {
        logger.Debug($"get_weapon_name: {(int)param_1}");

        return _FUN_007a0d10.orig_fptr(param_1);
    }
    private static void h_get_weapon_model(ushort name_id, byte owner, int unknown, ushort* model_id_pointer) {
        logger.Debug($"get_weapon_model: {name_id}, {owner}, {unknown}, {*model_id_pointer}, ");

        _FUN_007a0c70.orig_fptr(name_id, owner, unknown, model_id_pointer);
    }
    private static void h_obtain_treasure_cleanup(BtlRewardData* param_1, int param_2) {
        logger.Debug($"obtain_treasure_cleanup");

        _FUN_007993f0.orig_fptr(param_1, param_2);
    }

    public static void obtain_item(uint item_id, int amount=-1) {
        if (item_id == 0) return;
        var item_type = (item_id & 0xF000) >> 12;
        if (amount == -1) amount = (int)((item_id & 0xFF0000) >> 16);
        item_id &= 0xFFFF;
        int count;
        switch (item_type) {
            case 0xA:
                // Key Item
                h_TkMsImportantSet(item_id);
                // TODO: Handle Jecht Spheres and Al Bhed Primers
                break;
            case 0x2:
                // Item
                h_give_item(item_id, amount);
                break;
            case 0x5:
                // Equipment
                item_id &= 0xff;
                UnownedEquipment* weapon_data = (UnownedEquipment*)h_read_from_bin((int)item_id, (short*)(*buki_get_pointer), 0);
                //var data = get_from_bin((int)item_id, (short*)0x12000C00, 0);
                logger.Debug($"obtain_item: Weapon {(int)weapon_data}");
                if (weapon_data->is_celestial) {
                    foreach (var equip in Globals.save_data->equipment) {
                        if (equip.exists && equip.owner == weapon_data->owner && equip.is_celestial) {
                            // Upgrade celestial
                            if (celestial_level[equip.owner] < 2) {
                                celestial_level[equip.owner] += 1;
                                _TkSetLegendAbility.orig_fptr(equip.owner, celestial_level[equip.owner]);
                            }
                            return;
                        }
                    }
                } else if (weapon_data->is_brotherhood) {
                    foreach (ref var equip in Globals.save_data->equipment) {
                        if (equip.exists && equip.is_brotherhood) {
                            if (equip.is_hidden) {
                                // Obtain
                                equip.is_hidden = false;
                            } else {
                                // Upgrade
                                equip.flags = weapon_data->flags;
                                equip.owner = weapon_data->owner;
                                equip.type = weapon_data->type;
                                equip.dmg_formula = weapon_data->dmg_formula;
                                equip.power = weapon_data->power;
                                equip.crit_bonus = weapon_data->crit_bonus;
                                equip.slot_count = weapon_data->slot_count;
                                for (int i = 0; i < 4; i++) {
                                    equip.abilities[i] = weapon_data->abilities[i];
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
                new_weapon.flags = weapon_data->flags;
                new_weapon.owner = weapon_data->owner;
                new_weapon.type = weapon_data->type;
                new_weapon.equipped_by = 0xff;
                new_weapon.dmg_formula = weapon_data->dmg_formula;
                new_weapon.power = weapon_data->power;
                new_weapon.crit_bonus = weapon_data->crit_bonus;
                count = 0;
                for (int i = 0; i < 4; i++) {
                    if (weapon_data->abilities[i] == 0) {
                        new_weapon.abilities[i] = 0xff;
                    }
                    else {
                        new_weapon.abilities[i] = weapon_data->abilities[i];
                        count++;
                    }
                }
                new_weapon.slot_count = (byte)Math.Max(weapon_data->slot_count, count);
                new_weapon.name_id = h_get_weapon_name(&new_weapon);
                h_get_weapon_model(new_weapon.name_id, new_weapon.owner, 0, &new_weapon.model_id);
                var result = _FUN_007ab930(&new_weapon); // giveWeapon?
                if (result != 0) {
                    h_obtain_treasure_cleanup(&rewardData, 7);
                }
                break;
            case 0x1:
                // Gil
                if (amount == -1) amount = (int)((item_id & 0xFF0000) >> 16) * 1000;
                _MsPayGIL(-amount);
                break;
            case 0xE:
                // Region Unlock
                item_id &= 0xff;
                logger.Debug($"Region: {(RegionEnum)item_id}");
                region_is_unlocked[(RegionEnum)item_id] = true;
                break;
            case 0xF:
                // Party Member
                int char_id = (int)(item_id & 0xFF);
                logger.Debug($"Character: {id_to_character[char_id]}");
                unlocked_characters[char_id] = true;
                if (char_id == 0xF) {
                    // Magus sisters
                    unlocked_characters[PlySaveId.PC_MAGUS2] = true;
                    unlocked_characters[PlySaveId.PC_MAGUS3] = true;
                }
                save_party();
                reset_party();
                if (seed.GoalRequirement == GoalRequirement.PartyMembers || seed.GoalRequirement == GoalRequirement.PartyMembersAndAeons) {
                    int num_unlocked;
                    int num_required;
                    if (seed.GoalRequirement == GoalRequirement.PartyMembers) {
                        num_unlocked = unlocked_characters.Where(x => x.Key < 8 && x.Value).Count();
                        num_required = Math.Min(seed.RequiredPartyMembers, 8);
                    } else {
                        num_unlocked = unlocked_characters.Where(x => x.Key < 16 && x.Value).Count();
                        num_required = seed.RequiredPartyMembers;
                    }
                    string message = $"{num_unlocked}/{num_required} party members unlocked";
                    Color color = Color.White;
                    if (unlocked_characters.Count(x => x.Value) >= seed.RequiredPartyMembers) {
                        color = Color.Green;
                    }
                    ArchipelagoGUI.add_log_message([(message, color)]);
                }
                break;
            case 0x9:
                // Trap
                item_id &= 0xfff;
                logger.Debug($"Trap: {item_id}");
                if (item_id == 0) {
                    queued_voice_lines.Enqueue(voicelines[rng.Next(voicelines.Length)]);
                }
                break;
            case 0xC:
                // Other
                logger.Debug($"Other: {item_id}");
                other_inventory.TryGetValue(item_id, out count);
                other_inventory[item_id] = count+1;
                break;
        }
    }
    private static Random rng = new Random();

    public static void call_obtain_brotherhood() {
        AtelStack stack = new AtelStack();

        int param_1 = 0;
        int param_2 = 0;

        h_Common_obtainBrotherhoodRetInt((AtelBasicWorker*)&param_1, &param_2, &stack);
    }

    public static void receive_treasure(int id) {
        AtelStack stack = new AtelStack();
        stack.push_int(id);

        int param_1 = 0;
        int param_2 = 0;

        _Common_obtainTreasureSilentlyInit.orig_fptr((AtelBasicWorker*)&param_1, &param_2, &stack);
    }

    public static void set_airship_destinations() {
        logger.Debug("set_airship_destinations");
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

    public static void h_eiAbmParaGet() {
        // TODO: Replace normal calculation with custom Archipelago-based calculation (if option enabled)
        logger.Debug("Calculating stats");
        _eiAbmParaGet.orig_fptr();

        //foreach (ref PlySave ply in save_data->ply_saves) {
        //    ply.abi_map.has_extract_power   = true;
        //    ply.abi_map.has_extract_mana    = true;
        //    ply.abi_map.has_extract_speed   = true;
        //    ply.abi_map.has_extract_ability = true;
        //}
    }

    private static void h_MsSetRamChrParam(uint chr_id) {
        logger.Debug($"MsSetRamChrParam: {chr_id}");
        _MsSetRamChrParam.orig_fptr(chr_id);

        Chr* chr = _MsGetChr(chr_id);

        if (seed.AlwaysSensor == 1) {
            chr->ram.auto_ability_effects.has_sensor = true;
        }
        if (seed.AlwaysCapture == 1) {
            chr->ram.auto_ability_effects.has_capture = true;
        }
    }

    private static void h_MsSetSaveParam(uint chr_id) {
        logger.Debug("Calculating base stats and equipment");
        _MsSetSaveParam.orig_fptr(chr_id);

        // Does nothing??
        if (seed.AlwaysSensor == 1) {
            save_data->ply_saves[(int)chr_id].auto_ability_effects.has_sensor = true;
        }
        if (seed.AlwaysCapture == 1) {
            save_data->ply_saves[(int)chr_id].auto_ability_effects.has_capture = true;
        }

        return;
        //PlySave ply = save_data->ply_saves[(int)chr_id];
        //Equipment*[] equips =
        //[
        //    (Equipment*)_MsGetSaveWeapon(ply.wpn_inv_idx, 0x0),
        //    (Equipment*)_MsGetSaveWeapon(ply.arm_inv_idx, 0x0),
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
        //        AutoAbility* a_ability = (AutoAbility*)_MsGetExcelData(equip->abilities[i] & 0xFFF, Battle.btl->ptr_a_ability_bin, (int*)0x0);
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

    public static void h_FUN_00a48910(uint chr_id, int node_idx) {
        // TODO: Send Archipelago location when node is unlocked (if option enabled)
        logger.Debug($"Unlock node {node_idx} for {id_to_character[chr_id]}");
        _FUN_00a48910.orig_fptr(chr_id, node_idx);
    }

    private static uint h_MsApUp(int chr_id, Chr* chr, int base_ap_add, uint param_4) {
        logger.Debug($"AP gain: character={id_to_character[chr_id]}, ap={base_ap_add}");
        int new_ap = (int)Math.Min(Math.BigMul(base_ap_add, ap_multiplier), 999_999_999);
        return _MsApUp.orig_fptr(chr_id, chr, new_ap, param_4);
    }



    // Non-loading FMV
    private static bool h_graphicInitFMVPlayer(int movie_id, int param_2) {
        bool result = _graphicInitFMVPlayer.orig_fptr(movie_id, param_2);
        return !result;
    }

    // Texture experiments
    private static void h_FUN_00656c90(int param_1, int param_2, char* fileName) {
        //logger.Debug($"{param_1}, {param_2}, {(nint)fileName}");
        //
        //string nameString = Marshal.PtrToStringAnsi((nint)fileName);
        //
        //logger.Debug(nameString);


        _FUN_00656c90.orig_fptr(param_1, param_2, fileName);
    }

    private static void h_TkMenuAppearMainCmdWindow(int param_1, int param_2) {
        // All menu options are enabled at progress 0
        ushort progress = save_data->story_progress;
        save_data->story_progress = 0;
        _TkMenuAppearMainCmdWindow.orig_fptr(param_1, param_2);
        save_data->story_progress = progress;
    }

    //private static void h_FUN_0065ee30(FixedClusterData* data) {
    //    _FUN_0065ee30.orig_fptr(data);
    //
    //    //if (data->_0x0c != 0) {
    //    //    logger.Debug($"Data exists {(uint)data}");
    //    //
    //    //    nint* _this = (nint*)(data->_0x00 + data->_0x10);
    //    //
    //    //    var g_mainTexString = Marshal.StringToHGlobalAnsi("g_mainTex");
    //    //    var g_widthSizeString = Marshal.StringToHGlobalAnsi("g_widthSize");
    //    //    var g_heightSizeString = Marshal.StringToHGlobalAnsi("g_heightSize");
    //    //
    //    //    var g_mainTex    = h_FUN_0056cd50(*_this, g_mainTexString);
    //    //    var g_widthSize  = h_FUN_0056cd50(*_this, g_widthSizeString);
    //    //    var g_heightSize = h_FUN_0056cd50(*_this, g_heightSizeString);
    //    //
    //    //    if (g_mainTex != null) {
    //    //        logger.Debug($"g_mainTex exists {*g_mainTex}");
    //    //        if (g_widthSize != null) {
    //    //            logger.Debug($"g_widthSize exists {*g_widthSize}");
    //    //            if (g_heightSize != null) {
    //    //                logger.Debug($"g_heightSize exists {*g_heightSize}");
    //    //                if (_this[1] != 0) {
    //    //                    nint* _this2 = (nint*)_this[1];
    //    //                    byte[] widthBytes = new byte[g_widthSize->size];
    //    //                    for (int i = 0; i < widthBytes.Length; i++) {
    //    //                        widthBytes[i] = *(byte*)(_this2 + g_widthSize->offset + i);
    //    //                    }
    //    //                    logger.Debug("Copied width");
    //    //                    //FhModule.some_image_tex = g_mainTex;
    //    //                    //FhModule.some_image_dimensions.X = g_widthSize;
    //    //                    //FhModule.some_image_dimensions.Y = g_heightSize;
    //    //                }
    //    //            }
    //    //        }
    //    //    }
    //    //
    //    //    Marshal.FreeHGlobal(g_mainTexString);
    //    //    Marshal.FreeHGlobal(g_widthSizeString);
    //    //    Marshal.FreeHGlobal(g_heightSizeString);
    //    //}
    //}

    public static List<nint> loaded_clusters = [];
    public static PCluster* loadCluster(string file) {
        //var filePath = Marshal.StringToHGlobalAnsi("/FFX_Data/GameData/PS3Data/map/hiku/hiku22/2d/tex/D3D11/0_11_132_16_12.dds.phyre");
        if (file == "") return null;
        var filePath = Marshal.StringToHGlobalAnsi(file);
        nint clusterManager = FhUtil.get_at<nint>(0x008cca44);
        PCluster* cluster = _ClusterManager_loadPCluster(clusterManager, filePath);
        Marshal.FreeHGlobal(filePath);
        if (cluster == null) return null;
        loaded_clusters.Add((nint)cluster);

        int fixedClusterResult = _Phyre_PFramework_PApplication_FixupClusters(cluster, 1);
        if (fixedClusterResult != 0) return null;

        return cluster;
    }

    public static void releaseCluster(PCluster* cluster) {
        nint clusterManager = FhUtil.get_at<nint>(0x008cca44); //var clusterManager = getClusterManager();
        _ClusterManager_releasePCluster(clusterManager, cluster);

    }

    public static PCluster* getCluster(string file) {
        //var filePath = Marshal.StringToHGlobalAnsi("/FFX_Data/GameData/PS3Data/map/hiku/hiku22/2d/tex/D3D11/0_11_132_16_12.dds.phyre");
        if (file == "") return null;
        var filePath = Marshal.StringToHGlobalAnsi(file);
        nint unifiedFilePath = (nint)NativeMemory.AllocZeroed(0x100);
        _fiosUnifyFilename(filePath, unifiedFilePath, 0x100);
        nint clusterManager = FhUtil.get_at<nint>(0x008cca44);
        PCluster* cluster = _ClusterManager_getPClusterByName(clusterManager, unifiedFilePath);
        Marshal.FreeHGlobal(filePath);
        Marshal.FreeHGlobal(unifiedFilePath);

        return cluster;
    }

    public static PTexture2DBase* getTextureFromCluster(PCluster* cluster) {

        FixedClusterData data = new();
        data._0x14 = *(nint*)cluster + 0x1c;
        data._0x18 = *(nint*)data._0x14 != data._0x14 ? *(nint*)data._0x14 : 0;
        data._0x1c = (nint)FhUtil.ptr_at<nint>(0x008a3738); // PTexture2DBase vftable?
        _FUN_0065ee30(&data);
        if (data._0x0c == 0) {
            return null;
        }

        PTexture2DBase* _this = (PTexture2DBase*)(data._0x00 + data._0x10);
        return _this;

    }


    // Voice related
    //public static nint h_FMOD_EventSystem_load(nint param_1, nint file_path, nint param_3, nint param_4) {
    //    string path = Marshal.PtrToStringAnsi(file_path);
    //    nint result = _FMOD_EventSystem_load.orig_fptr(param_1, file_path, param_3, param_4);
    //    logger.Debug($"{path}, {param_1}, {param_3} -> {result}");

    //    return result;
    //}

    public static void h_FfxFmod_soundInit_setLang(nint ffxFmod, int lang) {
        _FfxFmod_soundInit_setLang.orig_fptr(ffxFmod, lang);
        *(byte*)(ffxFmod + 4) = 0;
    }

    // Unsure if there are side effects
    public static void h_LocalizationManager_Initialize(FFXLocalizationManager* localizationManager) {
        _LocalizationManager_Initialize.orig_fptr(localizationManager);
        if (TextLanguage.HasValue) {
            logger.Debug($"Text: {TextLanguage.Value}");
            localizationManager->text = (int)TextLanguage;
        }
        if (VoiceLanguage.HasValue) {
            logger.Debug($"Voice: {VoiceLanguage.Value}");
            localizationManager->video = (int)VoiceLanguage;
            localizationManager->voice = (int)VoiceLanguage;
        }
    }

    public static int h_FmodVoice_dataChange(nint FmodVoice, int event_id, nint param_2) {
        logger.Debug($"{FmodVoice}, {event_id}, {param_2}");
        int result = _FmodVoice_dataChange.orig_fptr(FmodVoice, event_id, param_2);
        //string bank_name = "ffx_us_voice03"; // Contains "Stay away from the summoner!" (136815042)
        foreach (string bank_name in (string[])["ffx_us_voice03", "ffx_us_voice07", "ffx_us_voice11", "ffx_us_voice12", "ffx_us_voice20"]) {
            string path = $"../../../FFX_Data/GameData/PS3Data/Sound_PC/Voice/US/{bank_name}.fev";
            nint file_path = Marshal.StringToHGlobalAnsi(path);

            int bank_index = bank_name[^1] + (bank_name[^2] * 5 - 0x108)*2;
            nint bank = *(int*)(FmodVoice+0x18) + bank_index*4;

            nint load_result = _FMOD_EventSystem_load(param_2, file_path, 0, bank);
            Marshal.FreeHGlobal(file_path);

            if (load_result == 0) {
                int* piVar5 = *(int**)bank;
                if (piVar5 != null) {
                    FMOD_Bank_Post_Load _FMOD_Bank_Post_Load = Marshal.GetDelegateForFunctionPointer<FMOD_Bank_Post_Load>(*(nint*)(*piVar5 + 8));

                    nint s_voice = Marshal.StringToHGlobalAnsi("voice");
                    load_result = _FMOD_Bank_Post_Load((nint)piVar5, s_voice, 0, *(int*)(FmodVoice + 0xc) + bank_index*4);
                    Marshal.FreeHGlobal(s_voice);
                    logger.Debug($"{bank_name}: {load_result}");
                }
            }

        }

        return result;
    }

    public static Queue<int> queued_voice_lines = [];
    public static void play_voice_line(int voice_line) {
        AtelStack stack = new AtelStack();
        stack.push_int(voice_line);
        logger.Debug($"stack_size = {stack.size}, voice_line = {stack.values.as_int()[0]}");

        int work = 0;
        int[] storage_array = [0,0,0,0];

        fixed (int* storage = storage_array) {
            _Common_playFieldVoiceLineInit((AtelBasicWorker*)&work, storage, &stack);
            _Common_playFieldVoiceLineExec((AtelBasicWorker*)&work, &stack);
            _Common_playFieldVoiceLineResultInt((AtelBasicWorker*)&work, storage, &stack);

            _Common_00D6Init((AtelBasicWorker*)&work, storage, &stack);
            _Common_00D6Exec((AtelBasicWorker*)&work, &stack);
            _Common_00D6ResultInt((AtelBasicWorker*)&work, storage, &stack);

        }

    }

    private static Dictionary<uint, string> item_name_cache = [];
    public static string get_item_name(uint item_id) {
        if (item_name_cache.TryGetValue(item_id, out var name)) return name;
        byte* item_name;
        _TkMsGetRomItem(item_id, (int*)&item_name);
        byte[] decoded = new byte[FhEncoding.compute_decode_buffer_size(new ReadOnlySpan<byte>(item_name, 1000))];
        int decoded_length = FhEncoding.decode(new ReadOnlySpan<byte>(item_name, 1000), decoded, flags:FhEncodingFlags.IMPLICIT_END);
        string decoded_string = Encoding.UTF8.GetString(decoded, 0, decoded_length);
        item_name_cache[item_id] = decoded_string;
        return decoded_string;
    }

    public static string get_other_item_name(uint item_id) {
        var item_type = (item_id & 0xF000) >> 12;
        var id = item_id & 0xFF;

        if (item_type != 0xC || !(id < other_item_names.Length)) return $"Unnamed item ({item_id})";
        return other_item_names[id];
    }

    private static Dictionary<int, CT_Exec>     cached_CT_Execs     = new();
    private static Dictionary<int, CT_RetInt>   cached_CT_RetInts   = new();
    private static Dictionary<int, CT_RetFloat> cached_CT_RetFloats = new();
    public static CT_Exec get_CT_Exec(int id) {
        if (cached_CT_Execs.TryGetValue(id, out var result)) {
            return result;
        }
        AtelCallTargetNamespace nmsp = (AtelCallTargetNamespace)(id >> 0xC);
        AtelCallTarget* internal_ct = nmsp.get_internal() + (id & 0xFFF);

        CT_Exec ct = Marshal.GetDelegateForFunctionPointer<CT_Exec>(internal_ct->ret_float_func);
        cached_CT_Execs[id] = ct;
        return ct;
    }
    public static CT_RetInt get_CT_RetInt(int id) {
        if (cached_CT_RetInts.TryGetValue(id, out var result)) {
            return result;
        }
        AtelCallTargetNamespace nmsp = (AtelCallTargetNamespace)(id >> 0xC);
        AtelCallTarget* internal_ct = nmsp.get_internal() + (id & 0xFFF);

        CT_RetInt ct = Marshal.GetDelegateForFunctionPointer<CT_RetInt>(internal_ct->ret_float_func);
        cached_CT_RetInts[id] = ct;
        return ct;
    }
    public static CT_RetFloat get_CT_RetFloat(int id) {
        if (cached_CT_RetFloats.TryGetValue(id, out var result)) {
            return result;
        }
        AtelCallTargetNamespace nmsp = (AtelCallTargetNamespace)(id >> 0xC);
        AtelCallTarget* internal_ct = nmsp.get_internal() + (id & 0xFFF);

        CT_RetFloat ct = Marshal.GetDelegateForFunctionPointer<CT_RetFloat>(internal_ct->ret_float_func);
        cached_CT_RetFloats[id] = ct;
        return ct;
    }


    // Custom namespace
    public void h_AtelInitTotal() {
        _logger.Debug("Initializing Atel namespaces");
        _AtelInitTotal.orig_fptr();

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
        SEND_PARTY_MEMBER_LOCATION,
        SEND_RECRUIT_LOCATION,
        BLOCK_WARP,
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
        IS_GOAL_UNLOCKED,
        REPLACE_ENTRY_POINT,
        RESTORE_ENTRY_POINT,
        LIGHTNING_DODGING,
    }

    static AtelCallTarget[] customNameSpace = {
        new() { ret_int_func = (nint)(delegate* unmanaged[Cdecl]<AtelBasicWorker*, int*, AtelStack*, int>)(&CT_RetInt_PushInt)},
        new() { ret_int_func = (nint)(delegate* unmanaged[Cdecl]<AtelBasicWorker*, int*, AtelStack*, int>)(&CT_RetInt_PushFloat)},
        new() { ret_int_func = (nint)(delegate* unmanaged[Cdecl]<AtelBasicWorker*, int*, AtelStack*, int>)(&CT_RetInt_IsCharacterUnlocked)},
        new() { ret_int_func = (nint)(delegate* unmanaged[Cdecl]<AtelBasicWorker*, int*, AtelStack*, int>)(&CT_RetInt_HasCelestialWeapon)},
        new() { ret_int_func = (nint)(delegate* unmanaged[Cdecl]<AtelBasicWorker*, int*, AtelStack*, int>)(&CT_RetInt_IsOtherLocationChecked)},
        new() { ret_int_func = (nint)(delegate* unmanaged[Cdecl]<AtelBasicWorker*, int*, AtelStack*, int>)(&CT_RetInt_IsTreasureLocationChecked)},
        new() { ret_int_func = (nint)(delegate* unmanaged[Cdecl]<AtelBasicWorker*, int*, AtelStack*, int>)(&CT_RetInt_CollectedPrimers)},
        new() { ret_int_func = (nint)(delegate* unmanaged[Cdecl]<AtelBasicWorker*, int*, AtelStack*, int>)(&CT_RetInt_SendPartyMemberLocation)},
        new() { ret_int_func = (nint)(delegate* unmanaged[Cdecl]<AtelBasicWorker*, int*, AtelStack*, int>)(&CT_RetInt_SendRecruitLocation)},
        new() { ret_int_func = (nint)(delegate* unmanaged[Cdecl]<AtelBasicWorker*, int*, AtelStack*, int>)(&CT_RetInt_BlockWarp)},
        new() { ret_int_func = (nint)(delegate* unmanaged[Cdecl]<AtelBasicWorker*, int*, AtelStack*, int>)(&CT_RetInt_RestoreInteraction)},
        new() { ret_int_func = (nint)(delegate* unmanaged[Cdecl]<AtelBasicWorker*, int*, AtelStack*, int>)(&CT_RetInt_SetAirshipDestinations)},
        new() { ret_int_func = (nint)(delegate* unmanaged[Cdecl]<AtelBasicWorker*, int*, AtelStack*, int>)(&CT_RetInt_ShowAirshipDestinations)},
        new() { ret_int_func = (nint)(delegate* unmanaged[Cdecl]<AtelBasicWorker*, int*, AtelStack*, int>)(&CT_RetInt_HideAirshipDestinations)},
        new() { ret_int_func = (nint)(delegate* unmanaged[Cdecl]<AtelBasicWorker*, int*, AtelStack*, int>)(&CT_RetInt_ShowCurrentAirshipLocation)},
        new() { ret_int_func = (nint)(delegate* unmanaged[Cdecl]<AtelBasicWorker*, int*, AtelStack*, int>)(&CT_RetInt_HideCurrentAirshipLocation)},
        new() { ret_int_func = (nint)(delegate* unmanaged[Cdecl]<AtelBasicWorker*, int*, AtelStack*, int>)(&CT_RetInt_TransitionToRegion)},
        new() { ret_int_func = (nint)(delegate* unmanaged[Cdecl]<AtelBasicWorker*, int*, AtelStack*, int>)(&CT_RetInt_Jump)},
        new() { ret_int_func = (nint)(delegate* unmanaged[Cdecl]<AtelBasicWorker*, int*, AtelStack*, int>)(&CT_RetInt_JumpIfFalse)},
        new() { ret_int_func = (nint)(delegate* unmanaged[Cdecl]<AtelBasicWorker*, int*, AtelStack*, int>)(&CT_RetInt_JumpIfTrue)},
        new() { ret_int_func = (nint)(delegate* unmanaged[Cdecl]<AtelBasicWorker*, int*, AtelStack*, int>)(&CT_RetInt_Offset)},
        new() { ret_int_func = (nint)(delegate* unmanaged[Cdecl]<AtelBasicWorker*, int*, AtelStack*, int>)(&CT_RetInt_OffsetIfFalse)},
        new() { ret_int_func = (nint)(delegate* unmanaged[Cdecl]<AtelBasicWorker*, int*, AtelStack*, int>)(&CT_RetInt_OffsetIfTrue)},
        new() { ret_int_func = (nint)(delegate* unmanaged[Cdecl]<AtelBasicWorker*, int*, AtelStack*, int>)(&CT_RetInt_UpdateRegionState)},
        new() { ret_int_func = (nint)(delegate* unmanaged[Cdecl]<AtelBasicWorker*, int*, AtelStack*, int>)(&CT_RetInt_LockPartyMember)},
        new() { ret_int_func = (nint)(delegate* unmanaged[Cdecl]<AtelBasicWorker*, int*, AtelStack*, int>)(&CT_RetInt_IsGoalUnlocked)},
        new() { ret_int_func = (nint)(delegate* unmanaged[Cdecl]<AtelBasicWorker*, int*, AtelStack*, int>)(&CT_RetInt_ReplaceEntryPoint)},
        new() { ret_int_func = (nint)(delegate* unmanaged[Cdecl]<AtelBasicWorker*, int*, AtelStack*, int>)(&CT_RetInt_RestoreEntryPoint)},
        new() { ret_int_func = (nint)(delegate* unmanaged[Cdecl]<AtelBasicWorker*, int*, AtelStack*, int>)(&CT_RetInt_LightningDodging)},
    };
    static GCHandle customNameSpaceHandle = GCHandle.Alloc(customNameSpace, GCHandleType.Pinned);

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static int CT_RetInt_PushInt(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        int high = atelStack->pop_int();
        int low  = atelStack->pop_int();
        int result = (high << 16) | low;
        logger.Debug($"PushInt: ({high}, {low}) => {result}");

        atelStack->push_int(result);
        return 1;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static int CT_RetInt_PushFloat(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        uint high = (uint)atelStack->pop_int() & 0xFFFF;
        uint low  = (uint)atelStack->pop_int() & 0xFFFF;
        float result = BitConverter.Int32BitsToSingle((int)((high << 16) | low));
        logger.Debug($"PushFloat: ({high}, {low}) => {result}");

        atelStack->push_float(result);
        return 1;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static int CT_RetInt_IsCharacterUnlocked(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        int character_id = atelStack->pop_int();
        logger.Debug($"IsCharacterUnlocked: {id_to_character[character_id]}");

        return is_character_unlocked(character_id) ? 1 : 0;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static int CT_RetInt_HasCelestialWeapon(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        int character_id = atelStack->pop_int();
        logger.Debug($"HasCelestialWeapon: {id_to_character[character_id]}");

        foreach (var equip in Globals.save_data->equipment) {
            if (equip.exists && equip.owner == character_id && equip.is_celestial) return 1;
        }

        return 0;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static int CT_RetInt_IsOtherLocationChecked(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        int other_id = atelStack->pop_int();
        logger.Debug($"IsOtherLocationChecked: {other_id | (long)ArchipelagoLocationType.Other}");

        return local_checked_locations.Contains(other_id | (long)ArchipelagoLocationType.Other) ? 1 : 0;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static int CT_RetInt_IsTreasureLocationChecked(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        int other_id = atelStack->pop_int();
        logger.Debug($"IsTreasureLocationChecked: {other_id | (long)ArchipelagoLocationType.Treasure}");

        return local_checked_locations.Contains(other_id | (long)ArchipelagoLocationType.Treasure) ? 1 : 0;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static int CT_RetInt_CollectedPrimers(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        int collected_primers = 0;
        for (int i = 0; i < 26; i++) {
            collected_primers += Globals.save_data->unlocked_primers.get_bit(i) ? 1 : 0;
        }
        logger.Debug($"CollectedPrimers: {collected_primers}");

        return collected_primers;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static int CT_RetInt_SendPartyMemberLocation(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        int partyMember_id =  atelStack->pop_int();
        if (!FFXArchipelagoClient.local_checked_locations.Contains(partyMember_id | (long)FFXArchipelagoClient.ArchipelagoLocationType.PartyMember)) {
            if (ArchipelagoFFXModule.item_locations.party_member.TryGetValue(partyMember_id, out var item)) {
                if (FFXArchipelagoClient.sendLocation(partyMember_id, FFXArchipelagoClient.ArchipelagoLocationType.PartyMember)) {
                    ArchipelagoFFXModule.obtain_item(item.id);
                }
            }
        }
        return 1;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static int CT_RetInt_SendRecruitLocation(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        int recruit_id =  atelStack->pop_int();
        if (!FFXArchipelagoClient.local_checked_locations.Contains(recruit_id | (long)FFXArchipelagoClient.ArchipelagoLocationType.Recruit)) {
            if (ArchipelagoFFXModule.item_locations.recruit.TryGetValue(recruit_id, out var item)) {
                if (FFXArchipelagoClient.sendLocation(recruit_id, FFXArchipelagoClient.ArchipelagoLocationType.Recruit)) {
                    ArchipelagoFFXModule.obtain_item(item.id);
                }
            }
        }
        return 1;
    }

    public static int atelTurnAround(Span<byte> dest, float x, float y, float z, ushort current_worker, ushort target_worker, ushort entry_point, float movementSpeed = 34.0f, float rotationSpeed1 = 0.10471976f, float rotationSpeed2 = 0.17453294f) {
        logger?.Debug($"x={x}, y={y}, z={z}");
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
    private static int   turnAroundScriptLength = atelTurnAround(Span<byte>.Empty, 0, 0, 0, 0, 0, 0);
    private static byte* turnAroundScript       = (byte*)NativeMemory.AllocZeroed((uint)turnAroundScriptLength);
    private static bool  savedCrossInteractionStatus;
    private static byte  savedInteractionFlags;

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static int CT_RetInt_BlockWarp(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        return blockWarp(work, storage, atelStack);
    }

    public static int blockWarp(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        int entrance = atelStack->pop_int();
        int map      = atelStack->pop_int();

        logger.Info($"Exit leads to other Region!");

        Chr* tidus = (Chr*)((int)Atel.controllers[0].worker(0) + 0x9c);
        if (tidus != null && tidus->actor != null) {
            Vector3 playerPos = tidus->actor->chr_pos_vec.AsVector3();
            var entrances = Atel.controllers[0].worker(0)->script_chunk->map_entrances.ToArray();
            var closestEntrance = Atel.controllers[0].worker(0)->script_chunk->map_entrances.ToArray()
                        .Select((e, i) => new {Index=i, Entrance=e, Distance=(playerPos - e.pos).Length()})
                        .MinBy(tuple => tuple.Distance);

            if (closestEntrance?.Distance < 200) {
                logger.Debug($"Entrance within 200: pos:({closestEntrance.Entrance.x}, {closestEntrance.Entrance.y}, {closestEntrance.Entrance.z}) distance:{closestEntrance.Distance}");
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
                _FUN_00867370((byte)AtelOp.REQEW & 0x7F, work, &work->threads[work->current_thread_priority], atelStack, 0);
                work->__0x34 = (ushort)(work->__0x34 & 0xEBFF | 0x800);
                //work->__0x34 = (ushort)(work->__0x34 | 0x800);
                atelStack->pop_int();
                //_FUN_008671d0((byte)AtelOp.REQ & 0x7F, &work->threads[work->current_thread_priority], work, atelStack);
                return 1;
            }
            logger.Debug($"Closest entrance: pos:({closestEntrance?.Entrance.x}, {closestEntrance?.Entrance.y}, {closestEntrance?.Entrance.z}) distance:{closestEntrance?.Distance}");
        }
        // Warp
        atelStack->push_int(382);
        atelStack->push_int(0);
        h_Common_warpToMap(work, storage, atelStack);
        return 1;

    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static int CT_RetInt_RestoreInteraction(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        int target_worker_index = atelStack->pop_int();
        AtelBasicWorker* target_worker = Atel.controllers[0].worker(target_worker_index);
        //if (savedCrossInteractionStatus) target_worker->field_interaction_flags |= 1 << 2; // Enable cross interaction
        target_worker->field_interaction_flags = savedInteractionFlags;
        return 1;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static int CT_RetInt_SetAirshipDestinations(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        set_airship_destinations();
        return 1;
    }
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static int CT_RetInt_ShowAirshipDestinations(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
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

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static int CT_RetInt_HideAirshipDestinations(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        for (uint i = 0; i < 15; i++) {
            customStringDrawInfos.Remove($"destination {i}");
        }
        return 1;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static int CT_RetInt_ShowCurrentAirshipLocation(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        logger.Debug("ShowCurrentAirshipLocation");
        customStringDrawInfos[$"current destination"] = new CustomStringDrawInfo(airship_destination_names[save_data->current_airship_location], new(52f, 73f), 1f);

        // AE0300 D80F80 call Map.?show2DLayer [800Fh](layerIndex=3 [03h]);
        atelStack->push_int(3);
        _Map_show2DLayerResultInt(work, storage, atelStack);
        return 1;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static int CT_RetInt_HideCurrentAirshipLocation(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        logger.Debug("HideCurrentAirshipLocation");
        customStringDrawInfos.Remove($"current destination");

        // AE0300 D81080 call Map.?hide2DLayer [8010h](layerIndex=3 [03h]);
        atelStack->push_int(3);
        _Map_hide2DLayerResultInt(work, storage, atelStack);
        return 1;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static int CT_RetInt_TransitionToRegion(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        RegionEnum region = (RegionEnum)atelStack->pop_int();

        if (current_region != RegionEnum.None) {
            update_region_state();
            current_region = RegionEnum.None;
            skip_state_updates = false;
        }

        current_region = region;
        int map = 0, entrance = 0;
        restore_region_state(ref map, ref entrance);
        logger.Debug($"Entering {region}: map={map}, entrance={entrance}");

        // Unlock locked characters. May not be necessary
        foreach (var character in ArchipelagoFFXModule.locked_characters) {
            ArchipelagoFFXModule.locked_characters[character.Key] = false;
        }
        save_party();
        reset_party();

        atelStack->push_int(map);
        atelStack->push_int(entrance);
        h_Common_transitionToMap(work, storage, atelStack);

        return 1;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static int CT_RetInt_Jump(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        int jump_index = atelStack->pop_int();
        work->current_thread.pc = (byte*)customScriptHandles[jump_index].AddrOfPinnedObject() - 3;
        return 1;
    }
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static int CT_RetInt_JumpIfFalse(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        int jump_index = atelStack->pop_int();
        int val = atelStack->pop_int();
        if (val == 0) {
            work->current_thread.pc = (byte*)customScriptHandles[jump_index].AddrOfPinnedObject() - 3;
        }
        return 1;
    }
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static int CT_RetInt_JumpIfTrue(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        int jump_index = atelStack->pop_int();
        int val = atelStack->pop_int();
        if (val == 1) {
            work->current_thread.pc = (byte*)customScriptHandles[jump_index].AddrOfPinnedObject() - 3;
        }
        return 1;
    }
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static int CT_RetInt_Offset(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        int offset = atelStack->pop_int();
        work->current_thread.pc += offset - 3;
        return 1;
    }
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static int CT_RetInt_OffsetIfFalse(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        int offset = atelStack->pop_int();
        int val = atelStack->pop_int();
        if (val == 0) {
            work->current_thread.pc += offset - 3;
        }
        return 1;
    }
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static int CT_RetInt_OffsetIfTrue(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        int offset = atelStack->pop_int();
        int val = atelStack->pop_int();
        if (val == 1) {
            work->current_thread.pc += offset - 3;
        }
        return 1;
    }
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static int CT_RetInt_UpdateRegionState(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        Vector3 playerPos = Globals.actors->chr_pos_vec.AsVector3();
        var closestEntrance = Atel.controllers[0].worker(0)->script_chunk->map_entrances.ToArray()
                    .Select((entrance, index) => new {Index=index, Entrance=entrance, Distance=(playerPos - entrance.pos).Length()})
                    .MinBy(tuple => tuple.Distance);
        if (closestEntrance?.Distance < 100) {
            last_entrance_id = (ushort)closestEntrance.Index;
            logger.Debug($"Entrance within 100: pos:({closestEntrance.Entrance.x}, {closestEntrance.Entrance.y}, {closestEntrance.Entrance.z}) distance:{closestEntrance.Distance}");
        }

        update_region_state(save_data->current_room_id, save_data->current_spawnpoint);
        refill_inventory();
        return 1;
    }
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static int CT_RetInt_LockPartyMember(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        int party_member = atelStack->pop_int();
        locked_characters[party_member] = true;
        if (party_member == PlySaveId.PC_MAGUS1) {
            locked_characters[PlySaveId.PC_MAGUS2] = true;
            locked_characters[PlySaveId.PC_MAGUS3] = true;
        }
        return 1;
    }
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static int CT_RetInt_IsGoalUnlocked(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        bool goal_requirement = false;
        bool primer_requirement = false;

        switch (seed.GoalRequirement) {
            case GoalRequirement.None:
                goal_requirement = true;
                break;
            case GoalRequirement.PartyMembers:
                if (unlocked_characters.Where(x => x.Key < 8 && x.Value).Count() >= Math.Min(seed.RequiredPartyMembers, 8))
                    goal_requirement = true;
                //if (unlocked_characters.All(c => c.Value)) {
                //    return 1;
                //}
                break;
            case GoalRequirement.PartyMembersAndAeons:
                if (unlocked_characters.Where(x => x.Key < 16 && x.Value).Count() >= seed.RequiredPartyMembers)
                    goal_requirement = true;
                break;
            case GoalRequirement.Pilgrimage:
                if (local_checked_locations.Contains(8 | (long)ArchipelagoLocationType.PartyMember) &&
                    local_checked_locations.Contains(9 | (long)ArchipelagoLocationType.PartyMember) &&
                    local_checked_locations.Contains(10 | (long)ArchipelagoLocationType.PartyMember) &&
                    local_checked_locations.Contains(11 | (long)ArchipelagoLocationType.PartyMember) &&
                    local_checked_locations.Contains(12 | (long)ArchipelagoLocationType.PartyMember) &&
                    local_checked_locations.Contains(37 | (long)ArchipelagoLocationType.Boss)) {
                    goal_requirement = true;
                }
                break;
            case GoalRequirement.Nemesis:
                if (local_checked_locations.Contains(83 | (long)ArchipelagoLocationType.Boss)) {
                    goal_requirement = true;
                }
                break;
        }

        if (seed.RequiredPrimers > 0) {
            int collected_primers = 0;
            for (int i = 0; i < 26; i++) {
                collected_primers += Globals.save_data->unlocked_primers.get_bit(i) ? 1 : 0;
            }

            if (collected_primers >= seed.RequiredPrimers)
                primer_requirement = true;
        }
        else
            primer_requirement = true;


        return goal_requirement && primer_requirement ? 1 : 0;
    }
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static int CT_RetInt_ReplaceEntryPoint(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
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
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static int CT_RetInt_RestoreEntryPoint(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
        int entryPoint = atelStack->pop_int();
        int workerIndex = atelStack->pop_int();
        if (originalEntryPoints.TryGetValue((workerIndex, entryPoint), out uint value)) {
            Atel.controllers[0].worker(workerIndex)->table_entry_points[entryPoint] = value;
        }
        return 1;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static int CT_RetInt_LightningDodging(AtelBasicWorker* work, int* storage, AtelStack* atelStack) {
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
        logger.Info($"Lightning Dodged: {current}");

        if (current > save_data->lightning_dodging_highest_consecutive_dodges)
            highestDodged = (ushort)current;

        string highestString = $"Highest: {highestDodged}";
        customStringDrawInfos["Lightning Highest Streak"] = new CustomStringDrawInfo(new ManagedCustomString(highestString), new(40f, 150f), 0.5f);
        logger.Info($"Highest Consecutive Dodged: {highestDodged}");

        return *dodged == 1 ? 1 : 0;
    }
}

//
//  TappGoShim.LiveActivities.swift
//  com.tapp.go.liveactivities
//
//  The Live Activity C entry points Unity's `DllImport("__Internal")` resolves against.
//
//  Marshalling and nothing else, exactly as core's `TappGoShim.swift` — read it for why a shim is a source
//  file, and for the three helpers used here (`tappGoPayload`, `tappGoHandOff`, `TappGoReplyTarget`), which
//  it declares. Unity compiles every Tapp shim into the same module, UnityFramework, so they are in scope.
//
//  Imports `Tapp` and nothing else, on purpose: the whole `TappBridge` facade lives in the core framework,
//  which finds the Live Activities module by its Obj-C class name at run time. So UnityFramework links one
//  native product however many Tapp packages are installed — the module itself is linked into the app, by
//  core's build hook, because this package asked for it.
//
//  The symbol names are a published contract — see docs/NATIVE-CONTRACT.md in the core repo.
//

import Foundation
import Tapp

// MARK: - Live Activities

@_cdecl("TappGo_StartLiveActivity")
public func TappGo_StartLiveActivity(
    _ json: UnsafePointer<CChar>?,
    _ context: UnsafeMutableRawPointer?,
    _ reply: TappGoReply?
) {
    let target = TappGoReplyTarget(context: context, reply: reply)
    TappBridge.startLiveActivity(json: tappGoPayload(json)) { target.send($0) }
}

@_cdecl("TappGo_UpdateLiveActivity")
public func TappGo_UpdateLiveActivity(
    _ json: UnsafePointer<CChar>?,
    _ context: UnsafeMutableRawPointer?,
    _ reply: TappGoReply?
) {
    let target = TappGoReplyTarget(context: context, reply: reply)
    TappBridge.updateLiveActivity(json: tappGoPayload(json)) { target.send($0) }
}

@_cdecl("TappGo_EndLiveActivity")
public func TappGo_EndLiveActivity(
    _ json: UnsafePointer<CChar>?,
    _ context: UnsafeMutableRawPointer?,
    _ reply: TappGoReply?
) {
    let target = TappGoReplyTarget(context: context, reply: reply)
    TappBridge.endLiveActivity(json: tappGoPayload(json)) { target.send($0) }
}

@_cdecl("TappGo_ActiveLiveActivities")
public func TappGo_ActiveLiveActivities() -> UnsafeMutablePointer<CChar>? {
    tappGoHandOff(TappBridge.activeLiveActivities())
}

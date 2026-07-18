// Copyright (C) 2026 SharpEmu Emulator Project
// SPDX-License-Identifier: GPL-2.0-or-later

namespace SharpEmu.HLE;

/// <summary>
/// Support for HLE synchronization primitives that block the guest thread's
/// host thread in place (inside the HLE call, on a host primitive) instead of
/// capturing a continuation and re-scheduling through the cooperative wake-key
/// machinery. In-place blocking makes block-and-wake atomic — the host
/// primitive owns the race — which removes the lost-wakeup window the
/// continuation path had between block registration and wake delivery.
/// </summary>
public static class GuestThreadBlocking
{
    /// <summary>
    /// Upper bound on a single host wait while a guest thread is parked. Waits
    /// are sliced so parked threads observe <see cref="ShutdownRequested"/>
    /// promptly at teardown; a wake via Monitor.Pulse still lands immediately.
    /// </summary>
    public const int WaitSliceMilliseconds = 50;

    private static volatile bool _shutdownRequested;

    // Guest thread handle -> what it is parked on. Populated only while a
    // thread is blocked (the slow path), read by the stall watchdog so
    // in-place-blocked threads are not reported as opaque "Running" threads.
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<ulong, string> _blockDescriptions = new();

    /// <summary>True once emulator teardown has begun; parked guest threads unwind.</summary>
    public static bool ShutdownRequested => _shutdownRequested;

    /// <summary>Called by the execution backend when guest execution is being torn down.</summary>
    public static void RequestShutdown() => _shutdownRequested = true;

    /// <summary>Records what the given guest thread is about to park on (diagnostics only).</summary>
    public static void NoteBlocked(ulong guestThreadHandle, string description)
    {
        if (guestThreadHandle != 0)
        {
            _blockDescriptions[guestThreadHandle] = description;
        }
    }

    /// <summary>Clears the parked-state note recorded by <see cref="NoteBlocked"/>.</summary>
    public static void NoteUnblocked(ulong guestThreadHandle)
    {
        if (guestThreadHandle != 0)
        {
            _blockDescriptions.TryRemove(guestThreadHandle, out _);
        }
    }

    /// <summary>What the thread is parked on, or null if it is not parked in place.</summary>
    public static string? DescribeBlock(ulong guestThreadHandle) =>
        _blockDescriptions.TryGetValue(guestThreadHandle, out var description) ? description : null;

    /// <summary>All currently parked threads (diagnostics; covers the primary thread too).</summary>
    public static KeyValuePair<ulong, string>[] SnapshotBlockDescriptions() => _blockDescriptions.ToArray();
}

using UnityEngine;
using System;

public static class GameEvents
{
    public static event Action<int> OnRoundChanged;
    public static event Action<float> OnWaveTimerChanged;
    public static event Action<int> OnPlayerHPChanged;
    public static event Action<int> OnSoulsChanged;
    public static event Action<int, int, int> OnExperienceChanged;
    public static event Action<Tier> OnShopTierChanged;
    public static event Action OnShopOpened;
    public static event Action OnShopClosed;
    public static event Action OnGameStarted;

    public static void RaiseRoundChanged(int round) => OnRoundChanged?.Invoke(round);
    public static void RaiseWaveTimerChanged(float timer) => OnWaveTimerChanged?.Invoke(timer);
    public static void RaisePlayerHPChanged(int hp) => OnPlayerHPChanged?.Invoke(hp);
    public static void RaiseSoulsChanged(int souls) => OnSoulsChanged?.Invoke(souls);
    public static void RaiseExperienceChanged(int exp, int reqExp, int level) => OnExperienceChanged?.Invoke(exp, reqExp, level);
    public static void RaiseShopTierChanged(Tier tier) => OnShopTierChanged?.Invoke(tier);
    public static void RaiseShopOpened() => OnShopOpened?.Invoke();
    public static void RaiseShopClosed() => OnShopClosed?.Invoke();
    public static void RaiseGameStarted() => OnGameStarted?.Invoke();
}
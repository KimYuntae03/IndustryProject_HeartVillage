#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
using System;
using System.Collections;
using Unity.Notifications;
using UnityEngine;

public sealed class MedicationReminders : MonoBehaviour
{
    private const string Message = "약 드실 시간이에요!";
    private const string Channel = "medication";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        var reminders = new GameObject(nameof(MedicationReminders));
        DontDestroyOnLoad(reminders);
        reminders.AddComponent<MedicationReminders>();
    }

    private IEnumerator Start()
    {
        var args = NotificationCenterArgs.Default;
        args.AndroidChannelId = Channel;
        args.AndroidChannelName = "복약 알림";
        args.AndroidChannelDescription = "설정한 복약 시간에 알립니다.";
        args.PresentationOptions = NotificationPresentation.Alert | NotificationPresentation.Sound;
        NotificationCenter.Initialize(args);
        var permission = NotificationCenter.RequestPermission();
        yield return permission;

#if UNITY_ANDROID
        using var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        using var activity = player.GetStatic<AndroidJavaObject>("currentActivity");
        using var receiver = new AndroidJavaClass("com.heartvillage.medication.MedicationAlarm");
        var minutes = new int[MedicationSchedule.Count];
        for (int i = 0; i < minutes.Length; i++)
            minutes[i] = (int)MedicationSchedule.TimeAt(i).TotalMinutes;
        receiver.CallStatic("configure", activity, minutes, Channel, Message);

        const string askedKey = "HeartVillage.Medication.ExactAlarmRequested";
        if (permission.Status == NotificationsPermissionStatus.Granted &&
            !Unity.Notifications.Android.AndroidNotificationCenter.UsingExactScheduling &&
            !PlayerPrefs.HasKey(askedKey))
        {
            PlayerPrefs.SetInt(askedKey, 1);
            PlayerPrefs.Save();
            Unity.Notifications.Android.AndroidNotificationCenter.RequestExactScheduling();
        }
#else
        for (int i = 0; i < MedicationSchedule.Count; i++)
        {
            var notification = new Notification
            {
                Identifier = 9400 + i,
                Title = Application.productName,
                Text = Message,
                ShowInForeground = true
            };
            var time = DateTime.Today + MedicationSchedule.TimeAt(i);
            NotificationCenter.ScheduleNotification(notification,
                new NotificationDateTimeSchedule(time, NotificationRepeatInterval.Daily));
        }
#endif
        Destroy(gameObject);
    }
}
#endif

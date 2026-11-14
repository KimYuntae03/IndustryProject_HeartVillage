package com.heartvillage.medication;

import android.app.AlarmManager;
import android.app.Notification;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.SharedPreferences;
import android.os.Build;
import java.util.Calendar;

public final class MedicationAlarm extends BroadcastReceiver {
    private static final String PREFS = "heartvillage_medication_alarms";
    private static final String DUE = "com.heartvillage.medication.DUE";

    public static void configure(Context context, int[] minutes, String channel, String message) {
        SharedPreferences prefs = context.getSharedPreferences(PREFS, Context.MODE_PRIVATE);
        SharedPreferences.Editor edit = prefs.edit().putInt("count", minutes.length)
                .putString("channel", channel).putString("message", message);
        for (int slot = 0; slot < minutes.length; slot++)
            edit.putInt("minutes" + slot, minutes[slot]);
        edit.apply();
        scheduleAll(context, prefs);
    }

    private static void scheduleAll(Context context, SharedPreferences prefs) {
        for (int slot = 0; slot < prefs.getInt("count", 0); slot++)
            scheduleNext(context, prefs, slot);
    }

    // Android repeating alarms are inexact. Arm the next local occurrence after each delivery.
    private static void scheduleNext(Context context, SharedPreferences prefs, int slot) {
        int minutes = prefs.getInt("minutes" + slot, 0);
        Calendar next = Calendar.getInstance();
        next.set(Calendar.HOUR_OF_DAY, minutes / 60);
        next.set(Calendar.MINUTE, minutes % 60);
        next.set(Calendar.SECOND, 0);
        next.set(Calendar.MILLISECOND, 0);
        if (next.getTimeInMillis() <= System.currentTimeMillis())
            next.add(Calendar.DATE, 1);

        long due = next.getTimeInMillis();
        Intent intent = new Intent(context, MedicationAlarm.class).setAction(DUE)
                .putExtra("slot", slot).putExtra("due", due);
        PendingIntent alarm = PendingIntent.getBroadcast(context, slot, intent,
                PendingIntent.FLAG_UPDATE_CURRENT | PendingIntent.FLAG_IMMUTABLE);
        AlarmManager manager = context.getSystemService(AlarmManager.class);
        if (Build.VERSION.SDK_INT < 31 || manager.canScheduleExactAlarms()) {
            try {
                manager.setExactAndAllowWhileIdle(AlarmManager.RTC_WAKEUP, due, alarm);
                return;
            } catch (SecurityException ignored) {
                // Exact-alarm access can change between the check and scheduling.
            }
        }
        manager.setAndAllowWhileIdle(AlarmManager.RTC_WAKEUP, due, alarm);
    }

    @Override public void onReceive(Context context, Intent intent) {
        SharedPreferences prefs = context.getSharedPreferences(PREFS, Context.MODE_PRIVATE);
        if (!DUE.equals(intent.getAction())) {
            scheduleAll(context, prefs);
            return;
        }
        int slot = intent.getIntExtra("slot", -1);
        if (slot < 0 || slot >= prefs.getInt("count", 0)) return;
        scheduleNext(context, prefs, slot);
        long elapsed = System.currentTimeMillis() - intent.getLongExtra("due", 0);
        NotificationManager manager = context.getSystemService(NotificationManager.class);
        if (elapsed < 0 || elapsed >= 30 * 60_000L || !manager.areNotificationsEnabled()) return;
        Notification.Builder builder = Build.VERSION.SDK_INT >= 26
                ? new Notification.Builder(context, prefs.getString("channel", "medication"))
                : new Notification.Builder(context).setPriority(Notification.PRIORITY_HIGH)
                        .setDefaults(Notification.DEFAULT_SOUND);
        builder.setSmallIcon(context.getApplicationInfo().icon)
                .setContentTitle(context.getApplicationInfo().loadLabel(context.getPackageManager()))
                .setContentText(prefs.getString("message", "")).setAutoCancel(true);
        Intent launch = context.getPackageManager().getLaunchIntentForPackage(context.getPackageName());
        if (launch != null) builder.setContentIntent(PendingIntent.getActivity(context, 0, launch,
                PendingIntent.FLAG_IMMUTABLE | PendingIntent.FLAG_UPDATE_CURRENT));
        manager.notify("medication", slot, builder.build());
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VehicleFramework.Admin
{
    public static class SessionManager
    {
        public static Coroutine StartCoroutine(IEnumerator coroutine)
        {
            if (coroutine == null)
            {
                throw Fatal("Input coroutine == null in SessionManager.StartCoroutine!");
            }
            if (MainPatcher.Instance == null)
            {
                ErrorMessage.AddError("MainPatcher Instance == null in SessionManager.StartCoroutine!");
                throw Fatal("MainPatcher Instance == null in SessionManager.StartCoroutine!");
            }
            return MainPatcher.Instance.StartCoroutine(coroutine);
        }
        public static void StopCoroutine(Coroutine? coroutine)
        {
            if (coroutine == null)
            {
                return;
            }
            if (MainPatcher.Instance == null)
            {
                throw Fatal("MainPatcher Instance == null in SessionManager.StopCoroutine!");
            }
            MainPatcher.Instance.StopCoroutine(coroutine);
        }

        internal static Exception Fatal(string message)
        {
            ErrorMessage.AddError("Vehicle Framework has encountered a fatal error and will not function.");
            ErrorMessage.AddError("Please see the error message below for more details:");
            ErrorMessage.AddError(message);
            Logger.Error("Vehicle Framework has encountered a fatal error and will not function.");
            Logger.Error("Please see the error message below for more details:");
            Logger.Error(message);
            return new Exception("Vehicle Framework Fatal Error: " + message);
        }
    }
}

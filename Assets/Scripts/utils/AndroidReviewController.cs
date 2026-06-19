// Copyright 2019 Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#if UNITY_ANDROID
using System;
using System.Collections;
using System.Collections.Generic;
using Google.Play.Review;
using UnityEngine;
using UnityEngine.UI;

public class AndroidReviewController : MonoBehaviour
{
    private static PlayReviewInfo _playReviewInfo;
    private ReviewManager _reviewManager;

    private bool isLoading = false;

    private void Start()
    {
        ViewModel.Instance.OnAndroidReviewRequest += OnAndroidReviewRequest;
    }

    void OnDestroy()
    {
        ViewModel.Instance.OnAndroidReviewRequest -= OnAndroidReviewRequest;
    }

    private void OnAndroidReviewRequest()
    {
        if (_reviewManager == null)
        {
            _reviewManager = new ReviewManager();
        }
        if (isLoading)
        {
            Debug.Log("Already loading review flow.");
            return;
        }
        StartCoroutine(AllInOneFlowCoroutine());
    }

    private IEnumerator RequestFlowCoroutine()
    {
        var requestFlowOperation = _reviewManager.RequestReviewFlow();
        yield return requestFlowOperation;
        if (requestFlowOperation.Error != ReviewErrorCode.NoError)
        {
            Debug.Log(requestFlowOperation.Error.ToString());
            AlternativeAppReview();
            yield break;
        }
        _playReviewInfo = requestFlowOperation.GetResult();
    }

    private IEnumerator LaunchFlowCoroutine()
    {
        yield return new WaitForSeconds(.1f);
        if (_playReviewInfo == null)
        {
            Debug.Log("PlayReviewInfo is null.");
            yield break;
        }
        var launchFlowOperation = _reviewManager.LaunchReviewFlow(_playReviewInfo);
        yield return launchFlowOperation;
        _playReviewInfo = null;
        if (launchFlowOperation.Error != ReviewErrorCode.NoError)
        {
            Debug.Log(launchFlowOperation.Error.ToString());
            AlternativeAppReview();
            yield break;
        }
    }

    private void AlternativeAppReview()
    {
        var appId = Application.identifier;
        Application.OpenURL("https://play.google.com/store/apps/details?id=" + appId);
    }

    private IEnumerator AllInOneFlowCoroutine()
    {
        if (isLoading)
        {
            yield break;
        }
        isLoading = true;
        yield return StartCoroutine(RequestFlowCoroutine());
        yield return StartCoroutine(LaunchFlowCoroutine());
        isLoading = false;
    }
}
#endif

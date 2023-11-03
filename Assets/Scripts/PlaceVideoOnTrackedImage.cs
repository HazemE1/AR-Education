using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARTrackedImageManager))]
public class PlaceVideoOnTrackedImage : MonoBehaviour
{
    private ARTrackedImageManager _trackedImagesManager;

    public GameObject[] ArPrefabs;

    private readonly Dictionary<string, GameObject>_instantiatedPrefabs = new Dictionary<string, GameObject>();

    void Awake(){
        _trackedImagesManager = GetComponent<ARTrackedImageManager>();
    }
    void onEnable(){
        _trackedImagesManager.trackedImagesChanged += OnTrackedImagesChanged;
    }
     void onDisable(){
        _trackedImagesManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }
    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventargs){ 
        foreach(var trackedImage in eventargs.added){
            var imageName = trackedImage.referenceImage.name;

            foreach (var curPrefab in ArPrefabs)
            {
                if(string.Compare(curPrefab.name, imageName, StringComparison.OrdinalIgnoreCase)== 0
                && !_instantiatedPrefabs.ContainsKey(imageName)){

                    var newPrefab = Instantiate(curPrefab, trackedImage.transform.position, trackedImage.transform.rotation);

                    _instantiatedPrefabs[imageName] = newPrefab;
                }
            }
        }
        foreach(var trackedImage in eventargs.updated){
            _instantiatedPrefabs[trackedImage.referenceImage.name].SetActive(trackedImage.trackingState == TrackingState.Tracking);
        }


        foreach(var trackedImage in eventargs.removed){
            Destroy(_instantiatedPrefabs[trackedImage.referenceImage.name]);

            _instantiatedPrefabs.Remove(trackedImage.referenceImage.name);
            
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

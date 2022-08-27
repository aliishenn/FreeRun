using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Curent;

    public float limitX;

    public float runningSpeed;
    public float xSpeed;
    float _currentRunningSpeed;

    public GameObject ridingCylinderPrefab;
    public List<RidingCylinder> cylinders;

    bool _spawninBridge;
    public GameObject bridgePiecePrefab;
    BridgeSpawner _bridgeSpawner;
    float _creatingBridgeTimer;

    private bool _finished;

    private float _scoreTimer = 0;

    public Animator animator;

    private float _lastTouchedX;
    private float _dropSoundTimer;

    public AudioSource cylinderAudioSound;
    public AudioClip gatherAudioClip, dropAudioClip;

    void Start()
    {
        Curent = this;
        
    }

    
    void Update()
    {
        if (LevelController.Current == null || !LevelController.Current.gameActive)
        {
            return;
        }
        float newX = 0;
        float touchXDelta = 0;

        if (Input.touchCount > 0 )
        {
            if (Input.GetTouch(0).phase == TouchPhase.Began)
            {
                _lastTouchedX = Input.GetTouch(0).position.x;
            }else if (Input.GetTouch(0).phase == TouchPhase.Moved)
            {
                touchXDelta = 8 * (_lastTouchedX - Input.GetTouch(0).position.x) / Screen.width;
                _lastTouchedX = Input.GetTouch(0).position.x;
            }
            touchXDelta = Input.GetTouch(0).deltaPosition.x / Screen.width;
        }
        else if (Input.GetMouseButton(0))
        {
            touchXDelta = Input.GetAxis("Mouse X");
        }

        newX = transform.position.x + xSpeed * touchXDelta * Time.deltaTime;
        newX = Mathf.Clamp(newX, -limitX, limitX);

        Vector3 newPosition = new Vector3(newX, transform.position.y, transform.position.z + _currentRunningSpeed * Time.deltaTime);
        transform.position = newPosition;

        if (_spawninBridge)
        {
            _creatingBridgeTimer -= Time.deltaTime;
            if (_creatingBridgeTimer < 0)
            {
                PlayDropSound();
                _creatingBridgeTimer = 0.01f;
                IncrementCylinderVolume(-0.01f);
                GameObject creatingBridgePiece = Instantiate(bridgePiecePrefab);
                Vector3 direction = _bridgeSpawner.endReference.transform.position - _bridgeSpawner.startReference.transform.position;
                float distance = direction.magnitude;
                direction = direction.normalized;
                creatingBridgePiece.transform.forward = direction;
                float characterDistance = transform.position.z - _bridgeSpawner.startReference.transform.position.z;
                characterDistance = Mathf.Clamp(characterDistance, 0, distance);
                Vector3 newPiecePosition = _bridgeSpawner.startReference.transform.position + direction * characterDistance;
                newPiecePosition.x = transform.position.x;
                creatingBridgePiece.transform.position = newPiecePosition;

                if (_finished)
                {
                    _scoreTimer -= Time.deltaTime;
                    if (_scoreTimer < 0)
                    {
                        _scoreTimer = 0.1f;
                        LevelController.Current.ChangeScore(+10);
                    }
                }
            }
        }
    }

    public void CahangeSpeed(float value)
    {
        _currentRunningSpeed = value;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "AddCylinder")
        {
            cylinderAudioSound.PlayOneShot(gatherAudioClip, 0.1f);
            IncrementCylinderVolume(0.1f);
            Destroy(other.gameObject);
        }
        else if (other.tag == "SpawnBridge")
        {
            StartSpawnBridge(other.transform.parent.GetComponent<BridgeSpawner>());
        }
        else if (other.tag == "StopSpawnBridge")
        {
            StopSpawnBridge();
            if (_finished)
            {
                LevelController.Current.FinishGame();
            }
        }else if (other.tag == "Finish")
        {
            _finished = true;
            StartSpawnBridge(other.transform.parent.GetComponent<BridgeSpawner>());
        }
        
    }

    private void OnTriggerStay(Collider other)
    {
        if (LevelController.Current.gameActive)
        {
            if (other.tag == "Trap")
            {
                PlayDropSound();
                IncrementCylinderVolume(-Time. fixedDeltaTime);
            }
        }
        
    }

    public void IncrementCylinderVolume(float value)
    {
        if (cylinders.Count == 0)
        {
            if (value > 0)
            {
                CreateCylinder(value);
            }
            else
            {
                if (_finished)
                {
                    LevelController.Current.FinishGame();
                }
                else
                {
                    Die();
                    
                }
            }
        }
        else
        {
            cylinders[cylinders.Count - 1].IncrementCylinderVolume(value);
        }
    }

    public void Die()
    {
        animator.SetBool("dead", true);
        gameObject.layer = 3;
        Camera.main.transform.SetParent(null);
        LevelController.Current.GameOver();
        //runningSpeed = 0;
        
        
    }

    public void CreateCylinder(float value)
    {
        RidingCylinder cretedCylinder = Instantiate(ridingCylinderPrefab, transform).GetComponent<RidingCylinder>();
        cylinders.Add(cretedCylinder);
        cretedCylinder.IncrementCylinderVolume(value);
    }

    public void DestroyCylinder(RidingCylinder cylinder)
    {
        cylinders.Remove(cylinder);
        Destroy(cylinder.gameObject);
        
    }

    public void StartSpawnBridge(BridgeSpawner spawner)
    {
        _bridgeSpawner = spawner;
        _spawninBridge = true;

    }

    public void StopSpawnBridge()
    {
        _spawninBridge = false;
    }

    public void PlayDropSound()
    {
        _dropSoundTimer -= Time.deltaTime;
        if (_dropSoundTimer < 0 )
        {
            _dropSoundTimer = 0.15f;
            cylinderAudioSound.PlayOneShot(dropAudioClip, 0.1f);
        }
    }
}

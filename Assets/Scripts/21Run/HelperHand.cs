using FRG.Taco;
using UnityEngine;

public class HelperHand : MonoBehaviour
{

    enum Direction
    {
        Left,
        Right
    }
    [SerializeField] GameObject handUp;
    [SerializeField] GameObject handDown;
    [SerializeField] Transform firstTransform;
    [SerializeField] Transform secondTransform;
    [SerializeField] Transform thirdTransform;
    [SerializeField] Transform fourthTransform;
    [SerializeField] Transform textLeftmost;
    [SerializeField] Transform textRightMost;
    [SerializeField] GameObject firstText;
    [SerializeField] GameObject secondText;

    Direction direction = Direction.Left;
    float textLerp = 0f;
    float lerp = 0f;
    float duration = 0.7f;
    float stayDuration = 0.5f;
    float stayingFor = 0f;
    int numberOfLanes = 4;
    int lane = 1;

    Vector3 startPosition;
    Vector3 endPosition;
    Vector3 textStartPosition;
    Vector3 textEndPosition;

    bool isMoving = true;

    void Awake() {
        startPosition = firstTransform.position;
        transform.position = startPosition;
        endPosition = secondTransform.position;
    }

    void Update()
    {
        if (isMoving)
        {
            Move();
            MoveText();
        }
        else
        {
            Click();
        }
    }

    void MoveText()
    {
        if (direction == Direction.Right)
        {
            textLerp = 1 -  (transform.position.x - firstTransform.position.x) / (fourthTransform.position.x - firstTransform.position.x);
        }
        else {
            textLerp = (fourthTransform.position.x - transform.position.x) / (fourthTransform.position.x - firstTransform.position.x);
        }
        
        firstText.transform.position = Vector3.Lerp(textLeftmost.position, textRightMost.position, textLerp);
        secondText.transform.position = Vector3.Lerp(textLeftmost.position, textRightMost.position, textLerp);
    }

    void Move()
    {
        handUp.SetActive(true);
        handDown.SetActive(false);
        lerp += Time.deltaTime / duration;
        transform.position = Vector3.Lerp(startPosition, endPosition, lerp);

        if (lerp >= 1f)
        {
            lerp = 0f;
            isMoving = false;

            if (lane < numberOfLanes - 1)
            {
                lane++;
            }
            else
            {
                lane = 0;
            }

            if (lane > 2)
            {
                firstText.SetActive(false);
                secondText.SetActive(true);
            }
            else if (lane != 0)
            {
                firstText.SetActive(true);
                secondText.SetActive(false);
            }
        }
    }


    void Click()
    {
        stayingFor += Time.deltaTime;
        if (stayingFor > stayDuration)
        {
            stayingFor = 0f;
            isMoving = true;
        }

        handUp.SetActive(false);
        handDown.SetActive(true);
        switch (lane)
        {
            case 0:
                endPosition = firstTransform.position;
                break;
            case 1:
                endPosition = secondTransform.position;
                break;
            case 2:
                endPosition = thirdTransform.position;
                break;
            case 3:
                endPosition = fourthTransform.position;
                break;
            default:
                Debug.LogError("HelperHand lane to animate to is out of bounds!");
                break;
        }
        startPosition = transform.position;
        direction = startPosition.x < endPosition.x ? Direction.Right : Direction.Left;
    }
}

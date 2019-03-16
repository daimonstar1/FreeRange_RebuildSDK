using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FRG.Taco.Run21
{
    public class TutorialPanels : MonoBehaviour, IBeginDragHandler, IEndDragHandler
    {
        enum Direction
        {
            Left,
            Right
        }

        [SerializeField] RectTransform canvasRectTransform;
        [SerializeField] Button leftButton;
        [SerializeField] Button rightButton;
        [SerializeField] Button exitButton;
        [SerializeField] Button[] circles;
        [SerializeField] Button[] enabledCircles;
        [SerializeField] float dragSpeed;
        [SerializeField] GameObject[] tutorialPages;


        public delegate void ClickAction();

        public event ClickAction OnClickedExit;

        Direction direction = Direction.Left;
        int page = 1; // human-readable page we're on
        bool isLerping;
        Vector3 startPosition;
        Vector3 endPosition;
        Vector3 distance;
        float duration;
        float lerp = 0f;
        int pagesSkipped = 0;
        const int numberOfScreens = 5;
        const int maxPosition = 0;
        int minPosition;
        int screenWidth;

        void OnEnable()
        {
            startPosition = transform.localPosition;
            endPosition = transform.localPosition;
            screenWidth = (int) canvasRectTransform.sizeDelta.x;
            page = 1;
            minPosition = -((numberOfScreens - 1) * screenWidth);

            for (int i = 0; i < numberOfScreens; i++)
            {
                tutorialPages[i].transform.localPosition = new Vector3(startPosition.x + i * screenWidth, startPosition.y, startPosition.z);
            }

            for (int i = 1; i < numberOfScreens; i++) // on Enable, first circle is shown (element 0). So we start on 1.
            {
                enabledCircles[i].gameObject.SetActive(false);
            }

            leftButton.gameObject.SetActive(false);
            rightButton.gameObject.SetActive(true);


            distance = new Vector3(screenWidth, 0f, 0f);
            isLerping = false;


            leftButton.onClick.AddListener(MoveLeft);
            rightButton.onClick.AddListener(MoveRight);
            exitButton.onClick.AddListener(ExitButtonClicked);

            for (int i = 0; i < circles.Length; i++)
            {
                circles[i].onClick.AddListener(CircleListener(i + 1));
            }

            for (int i = 0; i < enabledCircles.Length; i++)
            {
                enabledCircles[i].onClick.AddListener(CircleListener(i + 1));
            }
        }


        UnityAction CircleListener(int destinationPage)
        {
            return () => MoveToPage(destinationPage);
        }

        void MoveToPage(int destinationPage)
        {
            if (page == destinationPage)
            {
                return;
            }
            else if (page < destinationPage)
            {
                direction = Direction.Right;
                pagesSkipped = destinationPage - page - 1;
                endPosition = startPosition - (destinationPage - page) * distance;
                isLerping = true;
                lerp = 0f;
            }
            else if (page > destinationPage)
            {
                direction = Direction.Left;
                pagesSkipped = page - destinationPage - 1;
                endPosition = startPosition + (page - destinationPage) * distance;
                isLerping = true;
                lerp = 0f;
            }
        }

        void Update()
        {
            duration = screenWidth / dragSpeed; // in Start, needed also in Update if changed during play. Can be deleted in build.
            if (isLerping)
            {
                lerp += Time.deltaTime / duration;

                for (int i = 0; i < numberOfScreens; i++)
                {
                    Vector3 screenEndPosition = new Vector3(endPosition.x + i * screenWidth, endPosition.y, endPosition.z);
                    tutorialPages[i].transform.localPosition = Vector3.Lerp(tutorialPages[i].transform.localPosition, screenEndPosition, lerp);
                    if (Mathf.Approximately(tutorialPages[i].transform.localPosition.x, screenEndPosition.x))
                    {
                        HandleSwipeCompleted();
                    }
                }
            }

            switch (page)
            {
                case 1:
                    leftButton.gameObject.SetActive(false);
                    rightButton.gameObject.SetActive(true);
                    break;
                case 2:
                case 3:
                case 4:
                    leftButton.gameObject.SetActive(true);
                    rightButton.gameObject.SetActive(true);
                    break;
                case 5:
                    leftButton.gameObject.SetActive(true);
                    rightButton.gameObject.SetActive(false);
                    break;
                default:
                    Debug.LogError($"Tutorial page number is out of range, page: {page}");
                    break;
            }
        }

        private void HandleSwipeCompleted()
        {
            lerp = 0f;
            isLerping = false;
            startPosition = endPosition;

            if (direction == Direction.Left)
            {
                page--;
                if (pagesSkipped > 0)
                {
                    page -= pagesSkipped;
                    pagesSkipped = 0;
                }
            }
            else if (direction == Direction.Right)
            {
                page++;
                if (pagesSkipped > 0)
                {
                    page += pagesSkipped;
                    pagesSkipped = 0;
                }
            }

            for (int i = 0; i < numberOfScreens; i++)
            {
                if (i < page)
                {
                    enabledCircles[i].gameObject.SetActive(true);
                }
                else
                {
                    enabledCircles[i].gameObject.SetActive(false);
                }
            }
        }

        Vector2 startDragPosition;
        Vector2 endDragPosition;

        public void OnBeginDrag(PointerEventData data)
        {
            startDragPosition = data.position;
        }

        public void OnEndDrag(PointerEventData data)
        {
            endDragPosition = data.position;
            if (endDragPosition.x > startDragPosition.x)
            {
                MoveLeft();
            }
            else if (endDragPosition.x < startDragPosition.x)
            {
                MoveRight();
            }
        }

        void MoveLeft()
        {
            if (isLerping)
            {
                return;
            }

            if (startPosition.x >= maxPosition)
            {
                return;
            }

            direction = Direction.Left;
            endPosition = startPosition + distance;
            isLerping = true;
            lerp = 0f;
        }

        void MoveRight()
        {
            if (isLerping)
            {
                return;
            }

            if (startPosition.x <= minPosition)
            {
                return;
            }

            direction = Direction.Right;
            endPosition = startPosition - distance;
            isLerping = true;
            lerp = 0f;
        }

        void ExitButtonClicked()
        {
            if (OnClickedExit != null)
            {
                OnClickedExit();
            }

            gameObject.SetActive(false);
        }
    }
}
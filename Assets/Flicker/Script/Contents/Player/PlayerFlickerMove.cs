using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.VFX;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerFlickerMove : MonoBehaviour
{
    [Header("Flick")]
    public float power = 12f;
    public float maxDragDistance = 2.5f;
    public float stopThreshold = 0.15f;

    [Header("Aim Line(Optional)")]
    public LineRenderer line; //조준선
    public float lineLengthMultiplier = 1.2f;

    Rigidbody2D rb;
    Camera cam;
    bool dragging;
    Vector2 origin; //캐릭터 시작점 위치
    Vector2 current; //포인터 위치

    IHittable target;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;

        if(line != null)
        {
            line.positionCount = 2;
            line.enabled = false;
        }
    }

    private void Update()
    {
        //이동중 조작금지
        if (rb.velocity.sqrMagnitude > stopThreshold * stopThreshold) return;

        if(PointerDown(out Vector2 downPos))
        {
            if (isPointerOnUI()) return;

            var hit = Physics2D.OverlapPoint(downPos);
            if(hit != null && hit.attachedRigidbody == rb)
            {
                BeginDrag(downPos);
            }
        }
        if(dragging && PointerHeld(out Vector2 holdPos))
        {
            current = holdPos;
            UpdateLine();
        }
        if(dragging && PointerUp(out Vector2 upPos))
        {
            current = upPos;
            Release();
        }
    }
    bool PointerDown(out Vector2 pos)
    {
        //모바일 터치
        if (Input.touchCount > 0)
        {
            var t = Input.GetTouch(0);
            if(t.phase == TouchPhase.Began)
            {
                pos = cam.ScreenToWorldPoint(t.position);
                return true;
            }
        }
        //PC 클릭
        if (Input.GetMouseButtonDown(0))
        {
            pos = cam.ScreenToWorldPoint(Input.mousePosition);
            return true;
        }

        pos = default;
        return false;
    }
    bool PointerHeld(out Vector2 pos)
    {
        //모바일
        if (Input.touchCount > 0)
        {
            var t = Input.GetTouch(0);
            if(t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary)
            {
                pos = cam.ScreenToWorldPoint(t.position);
                return true;
            }
        }

        if (Input.GetMouseButton(0))
        {
            pos = cam.ScreenToWorldPoint(Input.mousePosition);
            return true;
        }

        pos = default;
        return false;
    }
    bool PointerUp(out Vector2 pos)
    {
        if (Input.touchCount > 0)
        {
            var t = Input.GetTouch(0);
            if(t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
            {
                pos = cam.ScreenToWorldPoint(t.position);
                return true;
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            pos = cam.ScreenToWorldPoint(Input.mousePosition);
            return true;
        }
        pos = default;
        return false;
    }
    void Release()
    {
        dragging = false;

        Vector2 delta = current - origin;
        float dist = Mathf.Min(delta.magnitude, maxDragDistance);
        //당기는힘 너무작으면 취소
        if (dist < 0.05f)
        {
            if (line != null) line.enabled = false;
            return;
        }

        Vector2 dir = -delta.normalized;
        float strength = dist / maxDragDistance;
        Vector2 impulse = dir * (power * strength);

        rb.AddForce(impulse, ForceMode2D.Impulse);

        if (line != null) line.enabled = false;
    }
    bool isPointerOnUI()
    {
        if (EventSystem.current == null) return false;

        //모바일
        if (Input.touchCount > 0)
        {
            return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
        }

        //PC
        return EventSystem.current.IsPointerOverGameObject();
    }
    void BeginDrag(Vector2 startPos)
    {
        dragging = true;
        origin = rb.position;
        current = startPos;

        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;

        if(line != null)
        {
            line.enabled = true;
            UpdateLine();
        }
    }
    void UpdateLine()
    {
        if (line == null) return;

        Vector2 delta = current - origin;
        float dist = Mathf.Min(delta.magnitude, maxDragDistance);

        Vector2 clamped = origin + delta.normalized * Mathf.Max(0.001f, dist);
        Vector2 shootDir = (origin - clamped).normalized;
        Vector2 end = origin + shootDir * (dist * lineLengthMultiplier);

        line.SetPosition(0, origin);
        line.SetPosition(1, end);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("충돌");
        if (collision.collider.TryGetComponent<IHittable>(out target))
        {
            rb.velocity = new Vector2(0, 0);
            target.Hit(10);
        }
    }
}

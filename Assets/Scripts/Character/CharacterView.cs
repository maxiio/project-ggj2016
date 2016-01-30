﻿using UnityEngine;

public class CharacterView : MonoBehaviour
{
    private CharacterSetting characterSetting = null;
    private SpriteRenderer m_spriteRenderer;
    private Animator animator = null;
    private Transform m_transform;
    AudioManager audioManager;

    private static readonly int NORMAL_HASH = Animator.StringToHash( "Normal" );
    private static readonly int DO_ACTION_HASH = Animator.StringToHash( "DoSpecificAction" );

    public void Initialize( CharacterSetting characterSetting )
    {
        this.characterSetting = characterSetting;
        m_spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponent<Animator>();
        m_transform = transform;
        audioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();


        ResetPosition();
        ResetAnim();
    }

    public void ResetPosition()
    {
        m_transform.localPosition = new Vector2(
            Random.Range( characterSetting.borderMin.x, characterSetting.borderMax.x ),
            Random.Range( characterSetting.borderMin.y, characterSetting.borderMax.y ) );
    }

    public void ResetAnim()
    {
        m_spriteRenderer.color = Color.white;
    }

    public void Translate( float x, float y )
    {
        Vector3 speedDirection = new Vector3( x, y, 0 );
        speedDirection = speedDirection.normalized;
        speedDirection *= characterSetting.moveSpeed;

        m_transform.Translate( speedDirection * Time.deltaTime );

        m_transform.localPosition = new Vector2(
            Mathf.Clamp( transform.localPosition.x, characterSetting.borderMin.x, characterSetting.borderMax.x ),
            Mathf.Clamp( transform.localPosition.y, characterSetting.borderMin.y, characterSetting.borderMax.y ) );

        m_transform.localPosition = new Vector3( transform.localPosition.x,
            transform.localPosition.y, transform.localPosition.y - characterSetting.borderMax.y );

        if ( x < 0f )
            m_transform.localScale = new Vector3( -1f, 1f, 1f );
        else if ( x > 0f )
            m_transform.localScale = Vector3.one;
    }

    public void FallingTranslate( float x, float y )
    {
        Vector3 speedDirection = new Vector3( x, y, 0 );
        speedDirection = speedDirection.normalized;
        speedDirection *= characterSetting.fallSpeed;

        m_transform.Translate( speedDirection * Time.deltaTime );

        m_transform.localPosition = new Vector2(
            Mathf.Clamp( transform.localPosition.x, characterSetting.borderMin.x, characterSetting.borderMax.x ),
            Mathf.Clamp( transform.localPosition.y, characterSetting.borderMin.y, characterSetting.borderMax.y ) );

        m_transform.localPosition = new Vector3( transform.localPosition.x,
            transform.localPosition.y, transform.localPosition.y - characterSetting.borderMax.y );
    }

    public float GetPushRange()
    {
        return characterSetting.pushRange;
    }

    public float GetPushTime()
    {
        return characterSetting.pushTime;
    }

    public float GetFallTime()
    {
        return characterSetting.fallTime;
    }

    public void DoAction( bool active )
    {
        if ( active )
        {
            animator.Play( DO_ACTION_HASH );
        }
        else
        {
            animator.Play( NORMAL_HASH );
        }
    }

    public void PlayPushAnim()
    {
        m_spriteRenderer.color = Color.yellow;
    }

    public void PlayFallAnim()
    {
        m_spriteRenderer.color = Color.green;
    }

    public void PlayDieAnim()
    {
        audioManager.PlaySound_CharDead();
        m_spriteRenderer.color = Color.blue;
    }
}
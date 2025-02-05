.data
ALIGN 16
ImageWidth  DQ 0
ImageHeight DQ 0
ImageStride DQ 0
MaxRadius   DQ 0.0
Power       DQ 0.0
CenterX     DQ 0.0
CenterY     DQ 0.0

; Constants
ALIGN 16
ONE         DQ 1.0, 1.0, 1.0, 1.0
TWO         DQ 2.0, 2.0, 2.0, 2.0
ZERO        DQ 0.0, 0.0, 0.0, 0.0
MASK_255    DD 255.0, 255.0, 255.0, 255.0

.code

PUBLIC ApplyVignette
ApplyVignette PROC
    push rbp
    mov rbp, rsp
    push rbx
    push r12
    push r13
    push r14
    push r15
    sub rsp, 32

    ; Save parameters with zero extension
    movsxd rdx, edx         ; ImageWidth
    mov [ImageWidth], rdx
    movsxd r8, r8d          ; ImageHeight
    mov [ImageHeight], r8
    movsxd r9, r9d          ; ImageStride
    mov [ImageStride], r9
    movsd [MaxRadius], xmm0

    ; Clamp Power between 0.0 and 1.0 before storing
    vmaxsd xmm1, xmm1, [ZERO]
    vminsd xmm1, xmm1, [ONE]
    movsd [Power], xmm1

    ; Calculate center coordinates
    mov rdx, [ImageWidth]
    vcvtsi2sd xmm0, xmm0, rdx
    vdivsd xmm0, xmm0, qword ptr [TWO]
    vmovsd [CenterX], xmm0

    mov r8, [ImageHeight]
    vcvtsi2sd xmm0, xmm0, r8
    vdivsd xmm0, xmm0, qword ptr [TWO]
    vmovsd [CenterY], xmm0

    ; Main processing loop
    xor r12, r12            ; row counter (y)
    mov r14, rcx            ; image data pointer

process_rows:
    cmp r12, [ImageHeight]
    jge done

    xor r15, r15            ; column pixel index (x)

process_pixels:
    cmp r15, [ImageWidth]
    jge next_row

    ; Calculate byte offset for current pixel
    mov rbx, r15
    imul rbx, 3

    ; Calculate dx = (x_pixel - CenterX)
    vcvtsi2sd xmm0, xmm0, r15
    vsubsd xmm0, xmm0, [CenterX]
    vmulsd xmm0, xmm0, xmm0      ; dx^2

    ; Calculate dy = (y_pixel - CenterY)
    vcvtsi2sd xmm1, xmm1, r12
    vsubsd xmm1, xmm1, [CenterY]
    vmulsd xmm1, xmm1, xmm1      ; dy^2

    vaddsd xmm0, xmm0, xmm1      ; dx^2 + dy^2
    vsqrtsd xmm0, xmm0, xmm0     ; distance

    ; Normalize distance by MaxRadius and multiply by clamped Power
    vdivsd xmm0, xmm0, [MaxRadius]
    vmulsd xmm0, xmm0, [Power]   ; Apply Power scaling

    ; Calculate vignette factor: max(0, 1 - (distance * Power))
    vmovsd xmm1, [ONE]
    vsubsd xmm0, xmm1, xmm0      ; 1 - (distance * Power)
    vmaxsd xmm0, xmm0, [ZERO]    ; Clamp to 0
    vminsd xmm0, xmm0, [ONE]     ; Clamp to 1

    ; Process BGR pixels
    movzx eax, byte ptr [r14 + rbx]      ; Blue
    movzx edx, byte ptr [r14 + rbx + 1]  ; Green
    movzx r8d, byte ptr [r14 + rbx + 2]  ; Red

    ; Apply vignette factor
    vcvtsi2sd xmm1, xmm1, eax
    vmulsd xmm1, xmm1, xmm0
    vcvtsd2si eax, xmm1

    vcvtsi2sd xmm1, xmm1, edx
    vmulsd xmm1, xmm1, xmm0
    vcvtsd2si edx, xmm1

    vcvtsi2sd xmm1, xmm1, r8d
    vmulsd xmm1, xmm1, xmm0
    vcvtsd2si r8d, xmm1

    ; Store results
    mov byte ptr [r14 + rbx], al
    mov byte ptr [r14 + rbx + 1], dl
    mov byte ptr [r14 + rbx + 2], r8b

    inc r15
    jmp process_pixels

next_row:
    add r14, [ImageStride]
    inc r12
    jmp process_rows

done:
    add rsp, 32
    pop r15
    pop r14
    pop r13
    pop r12
    pop rbx
    pop rbp
    ret

ApplyVignette ENDP

END
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

    ; Save parameters correctly with zero extension
    movsxd rdx, edx         ; Zero-extend image width to 64 bits
    mov [ImageWidth], rdx
    movsxd r8, r8d          ; Zero-extend image height
    mov [ImageHeight], r8
    movsxd r9, r9d          ; Zero-extend image stride
    mov [ImageStride], r9
    movsd [MaxRadius], xmm0
    movsd [Power], xmm1

    ; Calculate center coordinates using stored dimensions
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

    ; Calculate byte offset for current pixel (3 bytes per pixel)
    mov rbx, r15
    imul rbx, 3

    ; Calculate dx = (x_pixel - CenterX)
    vcvtsi2sd xmm0, xmm0, r15    ; x_pixel is r15
    vsubsd xmm0, xmm0, [CenterX]
    vmulsd xmm0, xmm0, xmm0      ; dx^2

    ; Calculate dy = (y_pixel - CenterY)
    vcvtsi2sd xmm1, xmm1, r12    ; current row (y_pixel)
    vsubsd xmm1, xmm1, [CenterY]
    vmulsd xmm1, xmm1, xmm1      ; dy^2

    vaddsd xmm0, xmm0, xmm1      ; dx^2 + dy^2
    vsqrtsd xmm0, xmm0, xmm0     ; distance

    ; Normalize distance by MaxRadius
    vdivsd xmm0, xmm0, [MaxRadius]

    ; Apply power: (distance / MaxRadius) ^ Power
    ; Assuming Power is a positive integer; for real exponents, a different approach is needed
    ; Example using exponentiation by squaring for integer Power (simplified here)
    ; Note: This part is a simplification and may need adjustment based on actual Power usage
    vmovsd xmm1, [Power]
    call pow_simulated            ; Custom function to compute xmm0^Power

    ; Calculate vignette factor: max(0, 1 - (distance^Power))
    vmovsd xmm2, [ONE]
    vsubsd xmm0, xmm2, xmm0       ; 1 - (distance^Power)
    vmaxsd xmm0, xmm0, [ZERO]     ; Clamp to 0
    vminsd xmm0, xmm0, [ONE]      ; Clamp to 1

    ; Process BGR pixels
    movzx eax, byte ptr [r14 + rbx]      ; Blue
    movzx edx, byte ptr [r14 + rbx + 1]  ; Green
    movzx r8d, byte ptr [r14 + rbx + 2]  ; Red

    ; Apply vignette factor to each channel
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
    mov byte ptr [r14 + rbx], al        ; Blue
    mov byte ptr [r14 + rbx + 1], dl    ; Green
    mov byte ptr [r14 + rbx + 2], r8b   ; Red

    ; Move to next pixel
    inc r15
    jmp process_pixels

next_row:
    add r14, [ImageStride]   ; Move to next row
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

pow_simulated:
    ; Simplified power function for demonstration (e.g., xmm0^Power where Power is 2)
    ; Replace with actual implementation based on Power's usage
    vmulsd xmm0, xmm0, xmm0
    ret

ApplyVignette ENDP

END
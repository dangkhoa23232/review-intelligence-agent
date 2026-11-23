import { useEffect, useState } from 'react'

/** Ảnh mặc định khi sản phẩm chưa có ảnh hoặc ảnh lỗi (frontend/public/images/san-pham/mac-dinh.webp). */
export const ANH_MAC_DINH = '/images/san-pham/mac-dinh.webp'

/**
 * Ảnh sản phẩm: ảnh gốc → ảnh mặc định → ô chữ cái đầu (nếu cả ảnh mặc định cũng lỗi).
 * Nền nhạt + object-cover giữ khung vuông cân đối với mọi tỉ lệ ảnh tải lên.
 */
export default function ProductImage({
  src,
  alt,
  className = '',
  uuTien = false,
}: {
  src: string | null | undefined
  alt: string
  className?: string
  /** Ảnh trên màn hình đầu tiên (ảnh chính trang chi tiết) → tải ngay, không lazy. */
  uuTien?: boolean
}) {
  const [buoc, setBuoc] = useState<0 | 1 | 2>(src ? 0 : 1)

  useEffect(() => setBuoc(src ? 0 : 1), [src])

  if (buoc === 2) {
    return (
      <div
        className={`flex items-center justify-center bg-gradient-to-br from-brand-50 to-brand-100 text-4xl font-semibold text-brand-500/60 ${className}`}
        role="img"
        aria-label={alt}
      >
        {alt.trim().charAt(0).toUpperCase()}
      </div>
    )
  }

  return (
    <img
      src={buoc === 0 ? src! : ANH_MAC_DINH}
      alt={alt}
      loading={uuTien ? 'eager' : 'lazy'}
      decoding="async"
      onError={() => setBuoc((b) => (b === 0 ? 1 : 2))}
      className={`bg-gray-50 object-cover ${className}`}
    />
  )
}

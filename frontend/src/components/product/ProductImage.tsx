import { useState } from 'react'

/** Ảnh sản phẩm có ảnh thay thế khi đường dẫn lỗi / chưa có ảnh. */
export default function ProductImage({ src, alt, className = '' }: { src: string | null | undefined; alt: string; className?: string }) {
  const [loi, setLoi] = useState(false)

  if (!src || loi) {
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

  return <img src={src} alt={alt} loading="lazy" onError={() => setLoi(true)} className={`object-cover ${className}`} />
}

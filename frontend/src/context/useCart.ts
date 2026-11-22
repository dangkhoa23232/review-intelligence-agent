import { useContext } from 'react'
import { CartContext, type CartState } from './CartContext'

export function useCart(): CartState {
  const ctx = useContext(CartContext)
  if (!ctx) throw new Error('useCart phải được dùng bên trong <CartProvider>.')
  return ctx
}

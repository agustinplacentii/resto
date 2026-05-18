import { StrictMode, useEffect, useMemo, useState } from 'react';
import { createRoot } from 'react-dom/client';
import {
  ArrowLeft,
  Check,
  FileText,
  Package,
  Plus,
  RefreshCw,
  ShoppingCart,
  Trash2,
  Utensils
} from 'lucide-react';
import './styles.css';

const API_ORIGIN = 'http://localhost:5088';
const API_URL = `${API_ORIGIN}/api`;

type ProductGroup = {
  id: number;
  name: string;
  description: string;
  productCount: number;
};

type Product = {
  id: number;
  name: string;
  category: string;
  measure: string;
  price: number;
  stock: number;
  isActive: boolean;
  productGroupId: number | null;
  productGroupName: string | null;
};

type OrderItem = {
  id: number;
  productId: number;
  productName: string;
  measure: string;
  quantity: number;
  unitPrice: number;
  subtotal: number;
};

type Order = {
  id: number;
  tableName: string;
  notes: string;
  total: number;
  status: 'Open' | 'Paid' | 'Cancelled';
  createdAt: string;
  items: OrderItem[];
  invoiceUrl: string;
};

type CartItem = {
  product: Product;
  quantity: number;
};

function money(value: number) {
  return new Intl.NumberFormat('es-AR', { style: 'currency', currency: 'ARS', maximumFractionDigits: 0 }).format(value);
}

function App() {
  const [groups, setGroups] = useState<ProductGroup[]>([]);
  const [products, setProducts] = useState<Product[]>([]);
  const [allProducts, setAllProducts] = useState<Product[]>([]);
  const [orders, setOrders] = useState<Order[]>([]);
  const [selectedGroup, setSelectedGroup] = useState<ProductGroup | null>(null);
  const [selectedProduct, setSelectedProduct] = useState<Product | null>(null);
  const [cart, setCart] = useState<CartItem[]>([]);
  const [tableName, setTableName] = useState('');
  const [notes, setNotes] = useState('');
  const [message, setMessage] = useState('');
  const [draft, setDraft] = useState({
    name: '',
    category: 'Pizzas',
    measure: 'Unidad',
    price: 0,
    stock: 0,
    productGroupId: 0
  });

  const total = useMemo(() => cart.reduce((sum, item) => sum + item.product.price * item.quantity, 0), [cart]);

  async function loadData(groupId = selectedGroup?.id) {
    const [groupsResponse, productsResponse, allProductsResponse, ordersResponse] = await Promise.all([
      fetch(`${API_URL}/products/groups`),
      fetch(groupId ? `${API_URL}/products?groupId=${groupId}` : `${API_URL}/products`),
      fetch(`${API_URL}/products`),
      fetch(`${API_URL}/orders`)
    ]);

    const nextGroups = await groupsResponse.json();
    setGroups(nextGroups);
    setProducts(await productsResponse.json());
    setAllProducts(await allProductsResponse.json());
    setOrders(await ordersResponse.json());

    if (draft.productGroupId === 0 && nextGroups.length > 0) {
      setDraft((current) => ({ ...current, productGroupId: nextGroups[0].id }));
    }
  }

  useEffect(() => {
    loadData().catch(() => setMessage('No pude conectar con la API. Revisa que el backend este corriendo.'));
  }, []);

  async function openGroup(group: ProductGroup) {
    setSelectedGroup(group);
    setSelectedProduct(null);
    setMessage('');

    const response = await fetch(`${API_URL}/products?groupId=${group.id}`);
    setProducts(await response.json());
  }

  async function openProduct(productId: number) {
    const response = await fetch(`${API_URL}/products/${productId}`);
    setSelectedProduct(await response.json());
  }

  function addToCart(product: Product) {
    if (!product.isActive || product.stock <= 0) {
      return;
    }

    setCart((items) => {
      const existing = items.find((item) => item.product.id === product.id);
      if (existing) {
        return items.map((item) =>
          item.product.id === product.id
            ? { ...item, quantity: Math.min(item.quantity + 1, product.stock) }
            : item
        );
      }

      return [...items, { product, quantity: 1 }];
    });
  }

  function updateCart(productId: number, quantity: number) {
    if (quantity <= 0) {
      setCart((items) => items.filter((item) => item.product.id !== productId));
      return;
    }

    setCart((items) =>
      items.map((item) =>
        item.product.id === productId
          ? { ...item, quantity: Math.min(quantity, item.product.stock) }
          : item
      )
    );
  }

  async function saveOrder() {
    setMessage('');

    const response = await fetch(`${API_URL}/orders`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        tableName,
        notes,
        items: cart.map((item) => ({ productId: item.product.id, quantity: item.quantity }))
      })
    });

    if (!response.ok) {
      const error = await response.json();
      setMessage(error.message ?? 'No se pudo guardar el pedido.');
      return;
    }

    const order: Order = await response.json();
    setCart([]);
    setTableName('');
    setNotes('');
    setMessage(`Pedido #${order.id} guardado. Ya podes imprimir la factura.`);
    await loadData(selectedGroup?.id);
    window.open(`${API_URL}/orders/${order.id}/invoice.pdf`, '_blank');
  }

  async function saveProduct(product: Product) {
    await fetch(`${API_URL}/products/${product.id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(product)
    });
    await loadData(selectedGroup?.id);
  }

  async function addProduct() {
    if (!draft.name.trim() || draft.productGroupId === 0) {
      return;
    }

    await fetch(`${API_URL}/products`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ ...draft, isActive: true })
    });

    setDraft({
      name: '',
      category: selectedGroup?.name ?? 'Pizzas',
      measure: 'Unidad',
      price: 0,
      stock: 0,
      productGroupId: selectedGroup?.id ?? groups[0]?.id ?? 0
    });
    await loadData(selectedGroup?.id);
  }

  async function updateStatus(order: Order, status: Order['status']) {
    await fetch(`${API_URL}/orders/${order.id}/status`, {
      method: 'PATCH',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ status })
    });
    await loadData(selectedGroup?.id);
  }

  return (
    <main>
      <header className="topbar">
        <div>
          <p className="eyebrow">Restaurant local</p>
          <h1>Pedidos, productos y factura</h1>
        </div>
        <button className="iconText" onClick={() => loadData()} title="Actualizar datos">
          <RefreshCw size={18} />
          Actualizar
        </button>
      </header>

      {message && <div className="notice">{message}</div>}

      <section className="workArea">
        <div className="panel menuPanel">
          <div className="sectionTitle">
            <Utensils size={20} />
            <h2>{selectedGroup ? selectedGroup.name : 'Categorias'}</h2>
          </div>

          {selectedGroup ? (
            <>
              <button className="backButton" onClick={() => { setSelectedGroup(null); setSelectedProduct(null); }}>
                <ArrowLeft size={18} />
                Volver a categorias
              </button>
              <div className="productGrid">
                {products.filter((product) => product.isActive).map((product) => (
                  <button className="productButton" key={product.id} onClick={() => openProduct(product.id)}>
                    <strong>{product.name}</strong>
                    <span>{product.measure}</span>
                    <b>{money(product.price)}</b>
                    <small>Stock {product.stock}</small>
                  </button>
                ))}
              </div>
            </>
          ) : (
            <div className="groupGrid">
              {groups.map((group) => (
                <button className="groupCard" key={group.id} onClick={() => openGroup(group)}>
                  <strong>{group.name}</strong>
                  <span>{group.description}</span>
                  <small>{group.productCount} productos</small>
                </button>
              ))}
            </div>
          )}
        </div>

        <aside className="panel detailPanel">
          <div className="sectionTitle">
            <Package size={20} />
            <h2>Detalle</h2>
          </div>
          {selectedProduct ? (
            <div className="productDetail">
              <p className="eyebrow">{selectedProduct.productGroupName ?? selectedProduct.category}</p>
              <h3>{selectedProduct.name}</h3>
              <span>{selectedProduct.measure}</span>
              <strong>{money(selectedProduct.price)}</strong>
              <p>Stock disponible: {selectedProduct.stock}</p>
              <button className="primary" onClick={() => addToCart(selectedProduct)} disabled={selectedProduct.stock <= 0}>
                <Plus size={18} />
                Sumar al carrito
              </button>
            </div>
          ) : (
            <p className="muted">Hace click en una categoria y despues en un producto para ver el detalle.</p>
          )}
        </aside>

        <aside className="panel cartPanel">
          <div className="sectionTitle">
            <ShoppingCart size={20} />
            <h2>Carrito</h2>
          </div>
          <label>
            Mesa o nombre
            <input value={tableName} onChange={(event) => setTableName(event.target.value)} placeholder="Mesa 4" />
          </label>
          <label>
            Notas
            <textarea value={notes} onChange={(event) => setNotes(event.target.value)} placeholder="Sin cebolla, para llevar..." />
          </label>
          <div className="cartItems">
            {cart.length === 0 && <p className="muted">Todavia no hay productos en el carrito.</p>}
            {cart.map((item) => (
              <div className="cartItem" key={item.product.id}>
                <div>
                  <strong>{item.product.name}</strong>
                  <span>{item.product.measure} - {money(item.product.price)} c/u</span>
                </div>
                <input
                  type="number"
                  min="1"
                  max={item.product.stock}
                  value={item.quantity}
                  onChange={(event) => updateCart(item.product.id, Number(event.target.value))}
                />
                <button className="iconOnly" onClick={() => updateCart(item.product.id, 0)} title="Quitar">
                  <Trash2 size={17} />
                </button>
              </div>
            ))}
          </div>
          <div className="totalRow">
            <span>Total</span>
            <strong>{money(total)}</strong>
          </div>
          <button className="primary" onClick={saveOrder} disabled={cart.length === 0}>
            <Check size={18} />
            Guardar e imprimir
          </button>
        </aside>
      </section>

      <section className="lowerGrid">
        <div className="panel">
          <div className="sectionTitle">
            <Package size={20} />
            <h2>Stock y precios</h2>
          </div>
          <div className="inventory">
            {allProducts.map((product) => (
              <div className="inventoryRow" key={product.id}>
                <input value={product.name} onChange={(event) => saveProduct({ ...product, name: event.target.value })} />
                <input value={product.measure} onChange={(event) => saveProduct({ ...product, measure: event.target.value })} />
                <input type="number" value={product.price} onChange={(event) => saveProduct({ ...product, price: Number(event.target.value) })} />
                <input type="number" value={product.stock} onChange={(event) => saveProduct({ ...product, stock: Number(event.target.value) })} />
                <label className="toggle">
                  <input type="checkbox" checked={product.isActive} onChange={(event) => saveProduct({ ...product, isActive: event.target.checked })} />
                  Activo
                </label>
              </div>
            ))}
            <div className="inventoryRow newProduct">
              <input value={draft.name} onChange={(event) => setDraft({ ...draft, name: event.target.value })} placeholder="Nuevo producto" />
              <input value={draft.measure} onChange={(event) => setDraft({ ...draft, measure: event.target.value })} placeholder="Medida" />
              <input type="number" value={draft.price} onChange={(event) => setDraft({ ...draft, price: Number(event.target.value) })} placeholder="Precio" />
              <input type="number" value={draft.stock} onChange={(event) => setDraft({ ...draft, stock: Number(event.target.value) })} placeholder="Stock" />
              <button className="iconText" onClick={addProduct}>
                <Plus size={18} />
                Agregar
              </button>
            </div>
          </div>
        </div>

        <div className="panel">
          <div className="sectionTitle">
            <FileText size={20} />
            <h2>Pedidos recientes</h2>
          </div>
          <div className="orders">
            {orders.map((order) => (
              <article className="orderCard" key={order.id}>
                <div className="orderHeader">
                  <strong>#{order.id} {order.tableName || 'Sin mesa'}</strong>
                  <span>{money(order.total)}</span>
                </div>
                <p>{order.items.map((item) => `${item.quantity} x ${item.productName}`).join(', ')}</p>
                {order.notes && <small>{order.notes}</small>}
                <div className="statusActions">
                  <button onClick={() => updateStatus(order, 'Open')} className={order.status === 'Open' ? 'selected' : ''}>Abierto</button>
                  <button onClick={() => updateStatus(order, 'Paid')} className={order.status === 'Paid' ? 'selected' : ''}>Pagado</button>
                  <button onClick={() => updateStatus(order, 'Cancelled')} className={order.status === 'Cancelled' ? 'selected' : ''}>Cancelado</button>
                  <a className="invoiceLink" href={`${API_ORIGIN}${order.invoiceUrl}`} target="_blank">Factura PDF</a>
                </div>
              </article>
            ))}
          </div>
        </div>
      </section>
    </main>
  );
}

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <App />
  </StrictMode>
);

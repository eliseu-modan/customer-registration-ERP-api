import { useEffect, useState } from "react";

const API_BASE_URL = (import.meta.env.VITE_API_BASE_URL);

const defaultCustomerForm = {
  name: "",
  document: "",
  email: "",
  phone: "",
  cep: "",
  street: "",
  number: "",
  neighborhood: "",
  city: "",
  state: ""
};

const defaultOrderForm = {
  customerId: "",
  items: [{ productId: "", quantity: 1 }]
};

function parseJwt(token) {
  try {
    const payload = token.split(".")[1];
    return JSON.parse(atob(payload.replace(/-/g, "+").replace(/_/g, "/")));
  } catch {
    return null;
  }
}

function App() {
  const [token, setToken] = useState(() => localStorage.getItem("erp_token") || "");
  const [session, setSession] = useState(() => {
    const saved = localStorage.getItem("erp_session");
    return saved ? JSON.parse(saved) : null;
  });
  const [activeTab, setActiveTab] = useState("dashboard");
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState("");

  const [loginForm, setLoginForm] = useState({ username: "admin", password: "admin123" });

  const [customers, setCustomers] = useState([]);
  const [customerForm, setCustomerForm] = useState(defaultCustomerForm);
  const [editingCustomerId, setEditingCustomerId] = useState(null);

  const [products, setProducts] = useState([]);
  const [orders, setOrders] = useState([]);
  const [orderForm, setOrderForm] = useState(defaultOrderForm);
  const [dashboard, setDashboard] = useState(null);

  const [cepSearch, setCepSearch] = useState("");
  const [cepResult, setCepResult] = useState(null);

  const role = session?.role || parseJwt(token)?.["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];

  useEffect(() => {
    if (!token) {
      return;
    }

    localStorage.setItem("erp_token", token);
    if (session) {
      localStorage.setItem("erp_session", JSON.stringify(session));
    }
  }, [token, session]);

  useEffect(() => {
    if (!token) {
      return;
    }

    void loadBootstrapData();
  }, [token]);

  async function apiFetch(path, options = {}) {
    const response = await fetch(`${API_BASE_URL}${path}`, {
      ...options,
      headers: {
        "Content-Type": "application/json",
        ...(token ? { Authorization: `Bearer ${token}` } : {}),
        ...(options.headers || {})
      }
    });

    if (response.status === 401) {
      logout();
      throw new Error("Sessão expirada. Faça login novamente.");
    }

    if (!response.ok) {
      let errorMessage = "Erro ao processar a requisição.";
      try {
        const body = await response.json();
        errorMessage = body.message || errorMessage;
      } catch {
        // ignore parse error
      }
      throw new Error(errorMessage);
    }

    if (response.status === 204) {
      return null;
    }

    return response.json();
  }

  async function loadBootstrapData() {
    setLoading(true);
    setMessage("");

    try {
      const [customerData, productData, orderData] = await Promise.all([
        apiFetch("/api/customers"),
        apiFetch("/api/products"),
        apiFetch("/api/orders")
      ]);

      setCustomers(customerData);
      setProducts(productData);
      setOrders(orderData);

      if (role === "Admin") {
        try {
          const dashboardData = await apiFetch("/api/dashboard");
          setDashboard(dashboardData);
        } catch (error) {
          setDashboard(null);
          setMessage(error.message);
        }
      } else {
        setDashboard(null);
      }
    } catch (error) {
      setMessage(error.message);
    } finally {
      setLoading(false);
    }
  }

  async function handleLogin(event) {
    event.preventDefault();
    setLoading(true);
    setMessage("");

    try {
      const response = await apiFetch("/api/auth/login", {
        method: "POST",
        body: JSON.stringify(loginForm)
      });

      setToken(response.token);
      setSession(response);
      setActiveTab(response.role === "Admin" ? "dashboard" : "customers");
      setMessage(`Login realizado com sucesso como ${response.role}.`);
    } catch (error) {
      setMessage(error.message);
    } finally {
      setLoading(false);
    }
  }

  function logout() {
    setToken("");
    setSession(null);
    setCustomers([]);
    setProducts([]);
    setOrders([]);
    setDashboard(null);
    localStorage.removeItem("erp_token");
    localStorage.removeItem("erp_session");
  }

  function handleCustomerChange(field, value) {
    setCustomerForm((current) => ({ ...current, [field]: value }));
  }

  async function lookupCepAndFill() {
    if (!customerForm.cep) {
      setMessage("Informe um CEP para buscar o endereço.");
      return;
    }

    setLoading(true);
    setMessage("");

    try {
      const address = await apiFetch(`/api/integrations/cep/${customerForm.cep}`);
      setCustomerForm((current) => ({
        ...current,
        street: address.street || current.street,
        neighborhood: address.neighborhood || current.neighborhood,
        city: address.city || current.city,
        state: address.state || current.state
      }));
      setMessage("Endereço preenchido automaticamente pelo ViaCEP.");
    } catch (error) {
      setMessage(error.message);
    } finally {
      setLoading(false);
    }
  }

  async function submitCustomer(event) {
    event.preventDefault();
    setLoading(true);
    setMessage("");

    try {
      const path = editingCustomerId ? `/api/customers/${editingCustomerId}` : "/api/customers";
      const method = editingCustomerId ? "PUT" : "POST";

      await apiFetch(path, {
        method,
        body: JSON.stringify(customerForm)
      });

      setCustomerForm(defaultCustomerForm);
      setEditingCustomerId(null);
      setMessage(editingCustomerId ? "Cliente atualizado." : "Cliente criado.");
      await loadBootstrapData();
    } catch (error) {
      setMessage(error.message);
    } finally {
      setLoading(false);
    }
  }

  function startEditCustomer(customer) {
    setEditingCustomerId(customer.id);
    setCustomerForm({
      name: customer.name,
      document: customer.document,
      email: customer.email,
      phone: customer.phone,
      cep: customer.cep,
      street: customer.street,
      number: customer.number,
      neighborhood: customer.neighborhood,
      city: customer.city,
      state: customer.state
    });
    setActiveTab("customers");
  }

  function updateOrderItem(index, field, value) {
    setOrderForm((current) => ({
      ...current,
      items: current.items.map((item, itemIndex) =>
        itemIndex === index ? { ...item, [field]: field === "quantity" ? Number(value) : value } : item
      )
    }));
  }

  function addOrderItem() {
    setOrderForm((current) => ({
      ...current,
      items: [...current.items, { productId: "", quantity: 1 }]
    }));
  }

  function removeOrderItem(index) {
    setOrderForm((current) => ({
      ...current,
      items: current.items.filter((_, itemIndex) => itemIndex !== index)
    }));
  }

  async function submitOrder(event) {
    event.preventDefault();
    setLoading(true);
    setMessage("");

    try {
      await apiFetch("/api/orders", {
        method: "POST",
        body: JSON.stringify(orderForm)
      });

      setOrderForm(defaultOrderForm);
      setMessage("Pedido criado com sucesso.");
      await loadBootstrapData();
    } catch (error) {
      setMessage(error.message);
    } finally {
      setLoading(false);
    }
  }

  async function updateOrderStatus(orderId, status) {
    setLoading(true);
    setMessage("");

    try {
      await apiFetch(`/api/orders/${orderId}/status`, {
        method: "PATCH",
        body: JSON.stringify({ status })
      });

      setMessage("Status do pedido atualizado.");
      await loadBootstrapData();
    } catch (error) {
      setMessage(error.message);
    } finally {
      setLoading(false);
    }
  }

  async function lookupCepOnly() {
    if (!cepSearch) {
      setMessage("Informe um CEP para consulta.");
      return;
    }

    setLoading(true);
    setMessage("");

    try {
      const result = await apiFetch(`/api/integrations/cep/${cepSearch}`);
      setCepResult(result);
      setMessage("Consulta realizada com sucesso.");
    } catch (error) {
      setCepResult(null);
      setMessage(error.message);
    } finally {
      setLoading(false);
    }
  }

  const orderPreviewTotal = orderForm.items.reduce((sum, item) => {
    const product = products.find((candidate) => candidate.id === item.productId);
    return sum + (product ? product.price * Number(item.quantity || 0) : 0);
  }, 0);

  if (!token) {
    return (
      <div className="login-shell">
        <div className="login-card">
          <p className="eyebrow">ERP Playground</p>
          <h1>Teste a API completa</h1>
          <p className="subtitle">
            Faça login e valide clientes, pedidos, dashboard e integração com ViaCEP.
          </p>

          <form className="stack" onSubmit={handleLogin}>
            <label>
              Usuário
              <input
                value={loginForm.username}
                onChange={(event) => setLoginForm((current) => ({ ...current, username: event.target.value }))}
                placeholder="admin"
              />
            </label>

            <label>
              Senha
              <input
                type="password"
                value={loginForm.password}
                onChange={(event) => setLoginForm((current) => ({ ...current, password: event.target.value }))}
                placeholder="admin123"
              />
            </label>

            <button className="primary-button" disabled={loading} type="submit">
              {loading ? "Entrando..." : "Entrar"}
            </button>
          </form>

          <div className="credentials">
            <strong>Credenciais seed</strong>
            <span>`admin / admin123`</span>
            <span>`funcionario / func123`</span>
          </div>

          {message ? <div className="feedback">{message}</div> : null}
        </div>
      </div>
    );
  }

  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div>
          <p className="eyebrow">ERP Frontend</p>
          <h2>Painel de Testes</h2>
          <p className="sidebar-copy">
            API em <code>{API_BASE_URL}</code>
          </p>
        </div>

        <div className="session-card">
          <span>{session?.name}</span>
          <strong>{session?.role}</strong>
          <small>{session?.username}</small>
        </div>

        <nav className="nav">
          {role === "Admin" ? (
            <button className={activeTab === "dashboard" ? "nav-item active" : "nav-item"} onClick={() => setActiveTab("dashboard")}>
              Dashboard
            </button>
          ) : null}
          <button className={activeTab === "customers" ? "nav-item active" : "nav-item"} onClick={() => setActiveTab("customers")}>
            Clientes
          </button>
          <button className={activeTab === "orders" ? "nav-item active" : "nav-item"} onClick={() => setActiveTab("orders")}>
            Pedidos
          </button>
          <button className={activeTab === "cep" ? "nav-item active" : "nav-item"} onClick={() => setActiveTab("cep")}>
            ViaCEP
          </button>
        </nav>

        <button className="ghost-button" onClick={logout}>
          Sair
        </button>
      </aside>

      <main className="content">
        <header className="topbar">
          <div>
            <h1>{tabTitle(activeTab)}</h1>
            <p>Use esta interface para validar os fluxos da API.</p>
          </div>

          <button className="ghost-button" onClick={() => void loadBootstrapData()}>
            Atualizar dados
          </button>
        </header>

        {message ? <div className="feedback">{message}</div> : null}

        {activeTab === "dashboard" && role === "Admin" ? (
          <section className="grid">
            <article className="stat-card">
              <span>Total de vendas</span>
              <strong>{formatCurrency(dashboard?.totalSales || 0)}</strong>
            </article>
            <article className="stat-card">
              <span>Total de pedidos</span>
              <strong>{dashboard?.totalOrders || 0}</strong>
            </article>
            <article className="panel">
              <h3>Pedidos por status</h3>
              <div className="status-list">
                {(dashboard?.ordersByStatus || []).map((item) => (
                  <div className="status-row" key={item.status}>
                    <span>{translateStatus(item.status)}</span>
                    <strong>{item.count}</strong>
                  </div>
                ))}
              </div>
            </article>
          </section>
        ) : null}

        {activeTab === "customers" ? (
          <section className="grid two-columns">
            <article className="panel">
              <h3>{editingCustomerId ? "Editar cliente" : "Novo cliente"}</h3>
              <form className="form-grid" onSubmit={submitCustomer}>
                {renderInput("Nome", customerForm.name, (value) => handleCustomerChange("name", value))}
                {renderInput("Documento", customerForm.document, (value) => handleCustomerChange("document", value))}
                {renderInput("Email", customerForm.email, (value) => handleCustomerChange("email", value))}
                {renderInput("Telefone", customerForm.phone, (value) => handleCustomerChange("phone", value))}
                {renderInput("CEP", customerForm.cep, (value) => handleCustomerChange("cep", value))}
                {renderInput("Número", customerForm.number, (value) => handleCustomerChange("number", value))}
                {renderInput("Rua", customerForm.street, (value) => handleCustomerChange("street", value))}
                {renderInput("Bairro", customerForm.neighborhood, (value) => handleCustomerChange("neighborhood", value))}
                {renderInput("Cidade", customerForm.city, (value) => handleCustomerChange("city", value))}
                {renderInput("UF", customerForm.state, (value) => handleCustomerChange("state", value))}

                <div className="button-row full-width">
                  <button className="ghost-button" onClick={lookupCepAndFill} type="button">
                    Buscar CEP
                  </button>
                  <button className="primary-button" disabled={loading} type="submit">
                    {editingCustomerId ? "Salvar edição" : "Cadastrar cliente"}
                  </button>
                </div>
              </form>
            </article>

            <article className="panel">
              <h3>Clientes cadastrados</h3>
              <div className="list">
                {customers.map((customer) => (
                  <div className="list-card" key={customer.id}>
                    <div>
                      <strong>{customer.name}</strong>
                      <p>{customer.document}</p>
                      <p>{customer.city} - {customer.state}</p>
                    </div>
                    <button className="ghost-button small" onClick={() => startEditCustomer(customer)}>
                      Editar
                    </button>
                  </div>
                ))}
              </div>
            </article>
          </section>
        ) : null}

        {activeTab === "orders" ? (
          <section className="grid two-columns">
            <article className="panel">
              <h3>Criar pedido</h3>
              <form className="stack" onSubmit={submitOrder}>
                <label>
                  Cliente
                  <select
                    value={orderForm.customerId}
                    onChange={(event) => setOrderForm((current) => ({ ...current, customerId: event.target.value }))}
                  >
                    <option value="">Selecione</option>
                    {customers.map((customer) => (
                      <option key={customer.id} value={customer.id}>
                        {customer.name}
                      </option>
                    ))}
                  </select>
                </label>

                <div className="stack">
                  {orderForm.items.map((item, index) => (
                    <div className="order-item-card" key={`${index}-${item.productId}`}>
                      <label>
                        Produto
                        <select
                          value={item.productId}
                          onChange={(event) => updateOrderItem(index, "productId", event.target.value)}
                        >
                          <option value="">Selecione</option>
                          {products.map((product) => (
                            <option key={product.id} value={product.id}>
                              {product.name} - {formatCurrency(product.price)}
                            </option>
                          ))}
                        </select>
                      </label>

                      <label>
                        Quantidade
                        <input
                          min="1"
                          type="number"
                          value={item.quantity}
                          onChange={(event) => updateOrderItem(index, "quantity", event.target.value)}
                        />
                      </label>

                      {orderForm.items.length > 1 ? (
                        <button className="ghost-button small" onClick={() => removeOrderItem(index)} type="button">
                          Remover
                        </button>
                      ) : null}
                    </div>
                  ))}
                </div>

                <div className="button-row">
                  <button className="ghost-button" onClick={addOrderItem} type="button">
                    Adicionar produto
                  </button>
                  <button className="primary-button" disabled={loading} type="submit">
                    Criar pedido
                  </button>
                </div>

                <div className="total-box">
                  <span>Total estimado</span>
                  <strong>{formatCurrency(orderPreviewTotal)}</strong>
                </div>
              </form>
            </article>

            <article className="panel">
              <h3>Pedidos</h3>
              <div className="list">
                {orders.map((order) => (
                  <div className="list-card order-card" key={order.id}>
                    <div>
                      <strong>{order.customerName}</strong>
                      <p>{formatCurrency(order.totalAmount)}</p>
                      <p>Status: {translateStatus(order.status)}</p>
                      <small>{new Date(order.createdAtUtc).toLocaleString("pt-BR")}</small>
                    </div>

                    <div className="order-actions">
                      <select
                        value={order.status}
                        onChange={(event) => void updateOrderStatus(order.id, event.target.value)}
                      >
                        <option value="Pending">Pendente</option>
                        <option value="Paid">Pago</option>
                        <option value="Cancelled">Cancelado</option>
                      </select>
                    </div>
                  </div>
                ))}
              </div>
            </article>
          </section>
        ) : null}

        {activeTab === "cep" ? (
          <section className="grid two-columns">
            <article className="panel">
              <h3>Consultar CEP</h3>
              <div className="stack">
                <label>
                  CEP
                  <input value={cepSearch} onChange={(event) => setCepSearch(event.target.value)} placeholder="01001000" />
                </label>
                <button className="primary-button" disabled={loading} onClick={lookupCepOnly} type="button">
                  Consultar
                </button>
              </div>
            </article>

            <article className="panel">
              <h3>Resultado</h3>
              {cepResult ? (
                <div className="result-box">
                  <p><strong>CEP:</strong> {cepResult.cep}</p>
                  <p><strong>Rua:</strong> {cepResult.street}</p>
                  <p><strong>Bairro:</strong> {cepResult.neighborhood}</p>
                  <p><strong>Cidade:</strong> {cepResult.city}</p>
                  <p><strong>UF:</strong> {cepResult.state}</p>
                </div>
              ) : (
                <p>Nenhuma consulta realizada ainda.</p>
              )}
            </article>
          </section>
        ) : null}
      </main>
    </div>
  );
}

function renderInput(label, value, onChange) {
  return (
    <label>
      {label}
      <input value={value} onChange={(event) => onChange(event.target.value)} />
    </label>
  );
}

function tabTitle(tab) {
  switch (tab) {
    case "dashboard":
      return "Dashboard";
    case "customers":
      return "Clientes";
    case "orders":
      return "Pedidos";
    case "cep":
      return "Integração ViaCEP";
    default:
      return "ERP";
  }
}

function formatCurrency(value) {
  return new Intl.NumberFormat("pt-BR", {
    style: "currency",
    currency: "BRL"
  }).format(value || 0);
}

function translateStatus(status) {
  if (status === "Pending") {
    return "Pendente";
  }
  if (status === "Paid") {
    return "Pago";
  }
  if (status === "Cancelled") {
    return "Cancelado";
  }
  return status;
}

export default App;

import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import ProductList from "./components/ProductList";
import Admin from "./components/Admin";
import Login from "./components/Login";

const App = () => {
    return (
        <Router>
            <Routes>
                <Route path="/" element={<Login />} />
                <Route path="/productlist" element={<ProductList />} />
                <Route path="/admin" element={<Admin userName="Justine" />} /> {/* Admin route */}
            </Routes>
        </Router>
    );

};

export default App;

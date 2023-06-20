import { Home } from "./components/Home";
import ViewGraphNG from "./components/ViewGraphNG";

const AppRoutes = [
  {
    index: true,
    element: <Home />,
  },
  {
    path: "/Graph",
    element: <ViewGraphNG />,
  },
];

export default AppRoutes;

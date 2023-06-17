import React, { Component } from "react";
import Container from "react-bootstrap/Container";
import Row from "react-bootstrap/Row";
import Stack from "react-bootstrap/Stack";
import Button from "react-bootstrap/Button";
import { NavLink } from "react-router-dom";
import { CreateWebsiteRecordModalWindow } from "./CreateWebsiteRecordModalWindow";

export class Home extends Component {
  static displayName = Home.name;

  constructor(props) {
    super(props);

    this.state = {
      isCreateWebsiteRecordModalShown: false,
      selectedGraphsIds: [21], // TODO
    };
  }

  onCreateWebsiteRecordModalClose = () =>
    this.setState({ isCreateWebsiteRecordModalShown: false });

  render() {
    return (
      <>
        <Container fluid="md">
          <Row>
            {/* TODO: Add the paginated view of website records here */}
          </Row>
          <Row>
            <Stack direction="horizontal" gap={3}>
              <Button
                variant="primary"
                onClick={() =>
                  this.setState({ isCreateWebsiteRecordModalShown: true })
                }
              >
                Create new website record
              </Button>

              <NavLink
                state={{ ids: this.state.selectedGraphsIds }}
                to="/Graph"
              >
                <Button variant="primary">View Graph</Button>
              </NavLink>
            </Stack>
          </Row>
        </Container>

        <CreateWebsiteRecordModalWindow
          show={this.state.isCreateWebsiteRecordModalShown}
          onClose={this.onCreateWebsiteRecordModalClose}
        />
      </>
    );
  }
}
